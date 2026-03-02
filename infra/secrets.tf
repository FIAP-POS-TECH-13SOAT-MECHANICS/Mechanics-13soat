resource "aws_secretsmanager_secret" "db_credentials" {
  name = "${local.prefix}-database"
}

resource "aws_secretsmanager_secret_version" "db_credentials_value" {
  secret_id = aws_secretsmanager_secret.db_credentials.id
  secret_string = jsonencode({
    value = "Server=${aws_db_instance.database.address},${aws_db_instance.database.port};Database=fiap-mechanics;User Id=${random_string.database_user.result};Password=${random_password.database_password.result};TrustServerCertificate=True;"
  })
}

resource "aws_secretsmanager_secret" "email_credentials" {
  name = "${local.prefix}-email"
}

resource "aws_secretsmanager_secret_version" "email_credentials_value" {
  secret_id = aws_secretsmanager_secret.email_credentials.id
  secret_string = jsonencode({
    userName = "${random_string.email_smtp_user.result}@${var.email_domain}"
    password = random_password.email_smtp_password.result
  })
}

resource "aws_secretsmanager_secret" "jwt_private_key" {
  name = "${local.prefix}-jwt/private-key"
}

resource "aws_secretsmanager_secret_version" "jwt_private_key" {
  secret_id     = aws_secretsmanager_secret.jwt_private_key.id
  secret_string = tls_private_key.jwt.private_key_pem
}

resource "aws_secretsmanager_secret" "jwt_public_key" {
  name = "${local.prefix}-jwt/public-key"
}

resource "aws_secretsmanager_secret_version" "jwt_public_key" {
  secret_id     = aws_secretsmanager_secret.jwt_public_key.id
  secret_string = tls_private_key.jwt.public_key_pem
}
