data "aws_caller_identity" "current" {}

resource "aws_ecr_repository" "container_registry" {
  name                 = "${local.prefix}-cr"
}
