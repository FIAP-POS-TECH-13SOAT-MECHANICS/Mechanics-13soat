resource "aws_secretsmanager_secret" "db_credentials" {
  count = var.public ? 0 : 1
  name  = "${var.project_name}-db-credentials"

  tags = {
    Project = var.project_name
  }
}

resource "aws_secretsmanager_secret_version" "db_credentials_value" {
  count     = var.public ? 0 : 1
  secret_id = aws_secretsmanager_secret.db_credentials[0].id
  secret_string = jsonencode({
    username = random_string.database_user.result
    password = random_string.database_password.result
  })
}

resource "aws_secretsmanager_secret" "email_credentials" {
  count = var.public ? 0 : 1
  name  = "${var.project_name}-email-credentials"
}

resource "aws_secretsmanager_secret_version" "email_credentials_value" {
  count     = var.public ? 0 : 1
  secret_id = aws_secretsmanager_secret.email_credentials[0].id
  secret_string = jsonencode({
    username = "${random_string.email_smtp_user.result}@${var.email.domain}"
    password = random_string.email_smtp_password.result
  })
}
