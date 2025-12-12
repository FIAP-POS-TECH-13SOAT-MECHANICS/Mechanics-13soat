data "aws_caller_identity" "current" {}

resource "aws_ecr_repository" "container_registry" {
  name                 = "${var.project_name}-cr"
  image_tag_mutability = "IMMUTABLE"
}
