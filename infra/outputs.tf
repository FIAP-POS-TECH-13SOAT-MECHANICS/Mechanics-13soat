output "db_instance_address" {
  value = aws_db_instance.database.address
}

output "db_instance_port" {
  value = aws_db_instance.database.port
}
