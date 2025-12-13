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
    public   = bool
  })
  default = {
    username = "sa"
    password = "$Y:2]SB$YoRu$jq"
    public = true
  }
  sensitive = true
}

variable "availability_zones" {
  description = "Availability Zones to be used in the VPC"
  type        = list(string)
  default     = ["us-east-1a", "us-east-1b"]
}

variable "vpc_cidr" {
  type        = string
  default     = "10.0.0.0/16"
}