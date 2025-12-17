# use "random_string" ao invés de "random_password" para facilitar outputs (ambiente público)

resource "random_string" "database_user" {
  length  = 12
  upper   = false
  numeric = false
  special = false
}

resource "random_string" "database_password" {
  length           = 16
  special          = true
  override_special = "!#$%&()-_=+[]{}<>?"
}

resource "random_string" "email_smtp_user" {
  length  = 10
  upper   = false
  numeric = false
  special = false
}

resource "random_string" "email_smtp_password" {
  length  = 16
  special = false
}

locals {
  email_smtp_auth = "${random_string.email_smtp_user.result}@${var.email.domain}:${random_string.email_smtp_password.result}"
}
