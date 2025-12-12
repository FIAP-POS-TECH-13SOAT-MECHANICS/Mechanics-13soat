output "db_instance_address" {
  value = aws_db_instance.database.address
}

output "db_instance_port" {
  value = aws_db_instance.database.port
}

output "cr_repository_url" {
  value = aws_ecr_repository.container_registry.repository_url
}
