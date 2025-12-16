output "db_connection_string" {
  value = var.public ? "Server=${aws_db_instance.database.address},${aws_db_instance.database.port};Database=fiap-mechanics;User Id=${var.db.username};Password=${var.db.password};TrustServerCertificate=True;" : null
}

output "cr_repository_url" {
  value = aws_ecr_repository.container_registry.repository_url
}

output "email_client_url" {
  value = aws_lb.mailpit_client.dns_name
}

output "email_smtp_url" {
  value = one(aws_lb.mailpit_smtp[*].dns_name)
}

output "email_smtp_port" {
  value = one(aws_lb_listener.mailpit_smtp[*].port)
}
