resource "aws_lb" "eks_nlb" {
  name               = "${local.prefix}-nlb"
  load_balancer_type = "network"
  internal           = false
  subnets            = aws_subnet.public[*].id
}

resource "aws_lb_target_group" "nginx_tg" {
  name     = "${local.prefix}-tg"
  port     = var.node_port
  protocol = "TCP"
  vpc_id   = aws_vpc.main.id

  target_type = "instance"

  health_check {
    protocol = "TCP"
  }
}

# listeners
resource "aws_lb_listener" "http" {
  load_balancer_arn = aws_lb.eks_nlb.arn
  port              = 80
  protocol          = "TCP"

  default_action {
    type             = "forward"
    target_group_arn = aws_lb_target_group.nginx_tg.arn
  }
}

# link ASG to target group
resource "aws_autoscaling_attachment" "asg_attachment" {
  for_each = {
    main = aws_eks_node_group.node_group.resources[0].autoscaling_groups[0].name
  }

  autoscaling_group_name = each.value
  lb_target_group_arn    = aws_lb_target_group.nginx_tg.arn
}

resource "aws_security_group_rule" "allow_nlb_http" {
  type              = "ingress"
  from_port         = 30080
  to_port           = 30080
  protocol          = "tcp"
  security_group_id = data.aws_eks_cluster.cluster.vpc_config[0].cluster_security_group_id
  cidr_blocks       = ["0.0.0.0/0"]
}
