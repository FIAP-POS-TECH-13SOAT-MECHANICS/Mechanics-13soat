variable "aws_region" {
  type    = string
  default = "us-east-1"
}

variable "project_name" {
  type    = string
  default = "fiap-mechanics"
}

variable "email" {
  type = object({
    image     = string
    domain    = string
  })
  default = {
    image  = "axllent/mailpit:v1.28"
    domain = "mechanics.com"
  }
}

variable "public" {
  type        = bool
  default     = false
  description = "If internal services - like database and SMTP server - will be publicly accessible"
}

variable "availability_zones" {
  description = "Availability Zones to be used in the VPC"
  type        = list(string)
  default     = ["us-east-1a", "us-east-1b"]
}

variable "vpc_cidr" {
  type    = string
  default = "10.0.0.0/16"
}
