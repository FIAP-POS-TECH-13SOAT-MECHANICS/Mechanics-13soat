resource "aws_ecs_cluster" "email" {
  name = "${local.prefix}-email"
}

# security group
resource "aws_security_group" "mailpit_web" {
  name        = "${local.prefix}-mailpit-web"
  description = "Security group for MailPit Web Client"
  vpc_id      = aws_vpc.main.id

  ingress {
    from_port   = 80
    to_port     = 80
    protocol    = "tcp"
    cidr_blocks = ["0.0.0.0/0"]
  }

  egress {
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    cidr_blocks = ["0.0.0.0/0"]
  }
}

resource "aws_security_group" "mailpit_ecs" {
  name        = "${local.prefix}-mailpit-sg"
  description = "Security group for MailPit ECS tasks"
  vpc_id      = aws_vpc.main.id

  # web-client
  ingress {
    from_port       = 80
    to_port         = 80
    protocol        = "tcp"
    security_groups = [aws_security_group.mailpit_web.id]
    description     = "MailPit Web Client"
  }

  # SMTP port
  ingress {
    from_port       = 1025
    to_port         = 1025
    protocol        = "tcp"
    security_groups = !local.public ? [data.aws_security_group.eks_cluster.id] : null
    cidr_blocks     = local.public ? ["0.0.0.0/0"] : null
    description     = "MailPit SMTP"
  }

  # health checks
  ingress {
    from_port   = 1025
    to_port     = 1025
    protocol    = "tcp"
    cidr_blocks = [var.vpc_cidr]
    description = "MailPit SMTP health check"
  }

  ingress {
    from_port   = 8025
    to_port     = 8025
    protocol    = "tcp"
    cidr_blocks = [var.vpc_cidr]
    description = "MailPit Web Client health check"
  }

  egress {
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    cidr_blocks = ["0.0.0.0/0"]
    description = "Allow all outbound traffic"
  }

  tags = {
    Name = local.public ? "mailpit-public-${var.environment}" : "mailpit-private-${var.environment}"
  }
}

# load balancer for web client
resource "aws_lb" "mailpit_client" {
  name               = "${local.prefix}-email-web"
  load_balancer_type = "application"
  subnets            = aws_subnet.public[*].id
  security_groups    = [aws_security_group.mailpit_web.id]
}

resource "aws_lb_target_group" "mailpit_client" {
  name        = "${local.prefix}-email-web"
  port        = 8025
  protocol    = "HTTP"
  target_type = "ip"
  vpc_id      = aws_vpc.main.id

  health_check {
    path = "/api/v1/info"
    port = "8025"
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
}

# load balancer for SMTP
resource "aws_lb" "mailpit_smtp" {
  name               = "${local.prefix}-email-smtp"
  load_balancer_type = "network"
  internal           = !local.public
  subnets            = local.public ? aws_subnet.public[*].id : aws_subnet.private[*].id
}

resource "aws_lb_target_group" "mailpit_smtp" {
  name        = "${local.prefix}-email-smtp"
  port        = 1025
  protocol    = "TCP"
  target_type = "ip"
  vpc_id      = aws_vpc.main.id

  health_check {
    protocol = "TCP"
    port     = "1025"
  }
}

resource "aws_lb_listener" "mailpit_smtp" {
  load_balancer_arn = aws_lb.mailpit_smtp.arn
  port              = 1025
  protocol          = "TCP"

  default_action {
    type             = "forward"
    target_group_arn = aws_lb_target_group.mailpit_smtp.arn
  }
}

# ECS Service
resource "aws_ecs_service" "email" {
  name            = "${local.prefix}-email-service"
  cluster         = aws_ecs_cluster.email.id
  task_definition = aws_ecs_task_definition.email.arn
  launch_type     = "FARGATE"
  desired_count   = 1

  network_configuration {
    subnets         = aws_subnet.private[*].id
    security_groups = [aws_security_group.mailpit_ecs.id]
  }

  load_balancer {
    target_group_arn = aws_lb_target_group.mailpit_client.arn
    container_name   = "mailpit-service"
    container_port   = 8025
  }

  load_balancer {
    target_group_arn = aws_lb_target_group.mailpit_smtp.arn
    container_name   = "mailpit-service"
    container_port   = 1025
  }

  depends_on = [aws_lb_listener.mailpit_client, aws_lb_listener.mailpit_smtp]
}

resource "aws_ecs_task_definition" "email" {
  family                   = "${local.prefix}-email"
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
          name  = "TZ"
          value = "America/Sao_Paulo"
        },
        {
          name  = "MP_SMTP_AUTH"
          value = "${random_string.email_smtp_user.result}@${var.email.domain}:${random_string.email_smtp_password.result}"
        },
        {
          name  = "MP_SMTP_AUTH_ALLOW_INSECURE"
          value = "true"
        }
      ]
      healthCheck = {
        command     = ["CMD-SHELL", "wget --spider -q http://localhost:8025/api/v1/info || exit 1"]
        interval    = 10
        timeout     = 5
        retries     = 3
        startPeriod = 5
      }
    }
  ])
}
