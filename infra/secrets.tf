resource "aws_secretsmanager_secret" "db_credentials" {
  count = local.public ? 0 : 1
  name  = "${local.prefix}/database"
}

resource "aws_secretsmanager_secret_version" "db_credentials_value" {
  count     = local.public ? 0 : 1
  secret_id = aws_secretsmanager_secret.db_credentials[0].id
  secret_string = "Server=${aws_db_instance.database.address},${aws_db_instance.database.port};Database=fiap-mechanics;User Id=${random_string.database_user.result};Password=${random_string.database_password.result};TrustServerCertificate=True;"
}

resource "aws_secretsmanager_secret" "email_credentials" {
  count = local.public ? 0 : 1
  name  = "${local.prefix}/email"
}

resource "aws_secretsmanager_secret_version" "email_credentials_value" {
  count     = local.public ? 0 : 1
  secret_id = aws_secretsmanager_secret.email_credentials[0].id
  secret_string = jsonencode({
    userName = "${random_string.email_smtp_user.result}@${var.email.domain}"
    password = random_string.email_smtp_password.result
  })
}
