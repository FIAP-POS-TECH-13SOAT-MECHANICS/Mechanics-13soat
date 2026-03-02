resource "random_string" "database_user" {
  length  = 12
  upper   = false
  numeric = false
  special = false
}

resource "random_password" "database_password" {
  length           = 16
  special          = true
  override_special = "!#$%&-_+"
}

resource "random_string" "email_smtp_user" {
  length  = 10
  upper   = false
  numeric = false
  special = false
}

resource "random_password" "email_smtp_password" {
  length  = 16
  special = false
}

resource "tls_private_key" "jwt" {
  algorithm = "RSA"
  rsa_bits  = 2048
}
