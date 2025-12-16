resource "aws_ecs_cluster" "email" {
  name = "${var.project_name}-email"
}

# security group
resource "aws_security_group" "mailpit" {
  name        = "${var.project_name}-mailpit-sg"
  description = "Security group for MailPit"
  vpc_id      = aws_vpc.main.id

  # web-client
  ingress {
    from_port   = 80
    to_port     = 80
    protocol    = "tcp"
    cidr_blocks = ["0.0.0.0/0"]
    description = "MailPit Web Client"
  }

  # SMTP port
  ingress {
    from_port   = 1025
    to_port     = 1025
    protocol    = "tcp"
    cidr_blocks = var.public ? ["0.0.0.0/0"] : [var.vpc_cidr]
    description = "MailPit SMTP"
  }

  # health check and redirects
  ingress {
    from_port   = 8025
    to_port     = 8025
    protocol    = "tcp"
    cidr_blocks = [var.vpc_cidr]
    description = "Internal access"
  }

  egress {
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    cidr_blocks = ["0.0.0.0/0"]
    description = "Allow all outbound traffic"
  }

  tags = {
    Project = var.project_name
    Service = "mailpit"
  }
}

# load balancer for web client
resource "aws_lb" "mailpit_client" {
  name               = "${var.project_name}-mailpit-client-lb"
  load_balancer_type = "application"
  subnets            = aws_subnet.public[*].id
  security_groups    = [aws_security_group.mailpit.id]

  tags = {
    Project = var.project_name
    Service = "mailpit"
  }
}

resource "aws_lb_target_group" "mailpit_client" {
  name        = "${var.project_name}-mailpit-client-tg"
  port        = 8025
  protocol    = "HTTP"
  target_type = "ip"
  vpc_id      = aws_vpc.main.id

  health_check {
    path = "/api/v1/info"
    port = "8025"
  }

  tags = {
    Project = var.project_name
    Service = "mailpit"
  }
}

resource "aws_lb_listener" "mailpit_client" {
  load_balancer_arn = aws_lb.mailpit_client.arn
  port              = 80
  protocol          = "HTTP"

  default_action {
    type             = "forward"
    target_group_arn = aws_lb_target_group.mailpit_client.arn
  }

  tags = {
    Project = var.project_name
    Service = "mailpit"
  }
}

# load balancer for SMTP (only if public)
resource "aws_lb" "mailpit_smtp" {
  count              = var.public ? 1 : 0
  name               = "${var.project_name}-mailpit-smtp-lb"
  load_balancer_type = "network"
  subnets            = var.public ? aws_subnet.public[*].id : aws_subnet.private[*].id
  security_groups    = [aws_security_group.mailpit.id]

  tags = {
    Project = var.project_name
    Service = "mailpit"
  }
}

resource "aws_lb_target_group" "mailpit_smtp" {
  count       = var.public ? 1 : 0
  name        = "${var.project_name}-mailpit-smtp-tg"
  port        = 1025
  protocol    = "TCP"
  target_type = "ip"
  vpc_id      = aws_vpc.main.id

  tags = {
    Project = var.project_name
    Service = "mailpit"
  }
}

resource "aws_lb_listener" "mailpit_smtp" {
  count = var.public ? 1 : 0

  load_balancer_arn = aws_lb.mailpit_smtp[0].arn
  port              = 1025
  protocol          = "TCP"

  default_action {
    type             = "forward"
    target_group_arn = aws_lb_target_group.mailpit_smtp[count.index].arn
  }

  tags = {
    Project = var.project_name
    Service = "mailpit"
  }
}

# ECS Service
resource "aws_ecs_service" "email" {
  name            = "${var.project_name}-email-service"
  cluster         = aws_ecs_cluster.email.id
  task_definition = aws_ecs_task_definition.email.arn
  launch_type     = "FARGATE"
  desired_count   = 1

  network_configuration {
    subnets          = aws_subnet.public[*].id
    security_groups  = [aws_security_group.mailpit.id]
    assign_public_ip = true
  }

  load_balancer {
    target_group_arn = aws_lb_target_group.mailpit_client.arn
    container_name   = "mailpit-service"
    container_port   = 8025
  }

  dynamic "load_balancer" {
    for_each = var.public ? [1] : []

    content {
      target_group_arn = aws_lb_target_group.mailpit_smtp[0].arn
      container_name   = "mailpit-service"
      container_port   = 1025
    }
  }

  depends_on = [aws_lb_listener.mailpit_client, aws_lb_listener.mailpit_smtp]

  tags = {
    Project = var.project_name
    Service = "mailpit"
  }
}

resource "aws_ecs_task_definition" "email" {
  family                   = "${var.project_name}-email"
  network_mode             = "awsvpc"
  requires_compatibilities = ["FARGATE"]
  cpu                      = "256"
  memory                   = "512"

  container_definitions = jsonencode([
    {
      name      = "mailpit-service"
      image     = var.email.image
      essential = true
      portMappings = [
        {
          containerPort = 8025
          hostPort      = 8025
        },
        {
          containerPort = 1025
          hostPort      = 1025
        }
      ]
      environment = [
        {
          name  = "MP_MAX_AGE"
          value = "7d"
        },
        {
          name  = "TZ"
          value = "America/Sao_Paulo"
        },
        {
          name  = "MP_SMTP_AUTH"
          value = var.email.auth
        },
        {
          name  = "MP_SMTP_AUTH_ALLOW_INSECURE"
          value = "true"
        }
      ]
    }
  ])

  tags = {
    Project = var.project_name
    Service = "mailpit"
  }
}
