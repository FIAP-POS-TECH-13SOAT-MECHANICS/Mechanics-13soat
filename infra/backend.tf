locals {
  tfstate_bucket_name = "fiap-mechanics-tf-${data.aws_caller_identity.current.account_id}"
}

data "aws_s3_bucket" "tfstate" {
  bucket = local.tfstate_bucket_name
}

resource "aws_s3_bucket_versioning" "versioning" {
  bucket = data.aws_s3_bucket.tfstate.id

  versioning_configuration {
    status = "Enabled"
  }
}

resource "aws_s3_bucket_public_access_block" "tfstate" {
  bucket = data.aws_s3_bucket.tfstate.id

  block_public_acls       = true
  block_public_policy     = true
  ignore_public_acls      = true
  restrict_public_buckets = true
}

resource "aws_s3_bucket_server_side_encryption_configuration" "tfstate" {
  bucket = data.aws_s3_bucket.tfstate.id

  rule {
    apply_server_side_encryption_by_default {
      sse_algorithm = "AES256"
    }
  }
}

terraform {
  backend "s3" {
    bucket  = local.tfstate_bucket_name
    region  = "us-east-1"
    encrypt = true

    dynamodb_table = "fiap-mechanics-tf"
  }
}
