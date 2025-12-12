resource "aws_s3_bucket" "tfstate" {
  bucket = "${var.project_name}-tf"

  lifecycle {
    prevent_destroy = true
  }

  tags = {
    Project = var.project_name
  }
}

resource "aws_s3_bucket_versioning" "versioning" {
  bucket = aws_s3_bucket.tfstate.id
  versioning_configuration {
    status = "Enabled"
  }
}

resource "aws_s3_bucket_public_access_block" "block" {
  bucket = aws_s3_bucket.tfstate.id

  block_public_acls       = true
  block_public_policy     = true
  ignore_public_acls      = true
  restrict_public_buckets = true
}

terraform {
  backend "s3" {
    bucket  = "soat-mechanics-tf"
    key     = "soat-mechanics/terraform.tfstate"
    region  = "us-east-1"
    encrypt = true
  }
}
