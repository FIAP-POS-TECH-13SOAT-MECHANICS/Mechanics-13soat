resource "aws_db_instance" "database" {
  identifier          = "${var.project_name}-db"
  allocated_storage   = 20
  engine              = "sqlserver-ex"
  engine_version      = "15.00"
  instance_class      = "db.t3.micro"
  username            = var.db.username
  password            = var.db.password
  storage_encrypted   = true
  timezone            = "E. South America Standard Time"
  storage_type        = "gp3"
  publicly_accessible = true
  skip_final_snapshot = true

  tags = {
    Project = var.project_name
  }
}
