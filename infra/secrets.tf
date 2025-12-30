resource "aws_secretsmanager_secret" "db_credentials" {
  name = "${local.prefix}-database"
}

resource "aws_secretsmanager_secret_version" "db_credentials_value" {
  secret_id = aws_secretsmanager_secret.db_credentials.id
  secret_string = jsonencode({
    value = "Server=${aws_db_instance.database.address},${aws_db_instance.database.port};Database=fiap-mechanics;User Id=${random_string.database_user.result};Password=${random_string.database_password.result};TrustServerCertificate=True;"
  })
}

resource "aws_secretsmanager_secret" "email_credentials" {
  name = "${local.prefix}-email"
}

resource "aws_secretsmanager_secret_version" "email_credentials_value" {
  secret_id = aws_secretsmanager_secret.email_credentials.id
  secret_string = jsonencode({
    userName = "${random_string.email_smtp_user.result}@${var.email_domain}"
    password = random_string.email_smtp_password.result
  })
}
