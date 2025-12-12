variable "aws_region" {
  type    = string
  default = "us-east-1"
}

variable "project_name" {
  type    = string
  default = "fiap-mechanics"
}

variable "db" {
  type = object({
    username = string
    password = string
  })
  default = {
    username = "sa"
    password = "$Y:2]SB$YoRu$jq"
  }
}
