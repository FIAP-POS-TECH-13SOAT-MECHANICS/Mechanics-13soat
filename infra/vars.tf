variable "aws_region" {
  type    = string
  default = "us-east-1"
}

variable "project_name" {
  type    = string
  default = "soat-mechanics"
}

variable "db" {
  type = object({
    db_name  = string
    username = string
    password = string
  })
  default = {
    db_name  = "soat-mechanics"
    username = "sa"
    password = "$Y:2]SB$YoRu$jq"
  }
}
