output "environment" {
  value = local.environment_name
}

output "db_connection_string" {
  value = local.public ? "Server=${aws_db_instance.database.address},${aws_db_instance.database.port};Database=fiap-mechanics;User Id=${random_string.database_user.result};Password=${random_string.database_password.result};TrustServerCertificate=True;" : null
}

output "cr_repository_url" {
  value = aws_ecr_repository.container_registry.repository_url
}

output "email_smtp_user" {
  value = local.public ? "${random_string.email_smtp_user.result}@${var.email_domain}" : null
}

output "email_smtp_password" {
  value = local.public ? random_string.email_smtp_password.result : null
}
