resource "aws_db_instance" "database" {
  identifier              = "${var.project_name}-db"
  allocated_storage       = 20
  engine                  = "sqlserver-ex"
  engine_version          = "15.00"
  instance_class          = "db.t3.small"
  username                = var.db.username
  password                = var.db.password
  storage_encrypted       = true
  timezone                = "E. South America Standard Time"
  storage_type            = "gp3"
  publicly_accessible     = var.public
  skip_final_snapshot     = true
  apply_immediately       = true
  backup_retention_period = 0

  db_subnet_group_name   = aws_db_subnet_group.mssql.name
  vpc_security_group_ids = [aws_security_group.mssql.id]

  tags = {
    Project = var.project_name
  }
}

resource "aws_db_subnet_group" "mssql" {
  name        = "${var.project_name}-db"
  description = "Subnet group for RDS"

  subnet_ids = var.public ? aws_subnet.public[*].id : aws_subnet.private[*].id

  tags = {
    Project = var.project_name
  }
}

resource "aws_security_group" "mssql" {
  name        = "${var.project_name}-mssql-sg"
  description = "Security group for SQL Server"
  vpc_id      = aws_vpc.main.id

  ingress {
    from_port   = 1433
    to_port     = 1433
    protocol    = "tcp"
    cidr_blocks = var.public ? ["0.0.0.0/0"] : [var.vpc_cidr]
    description = var.public ? "SQL Server - Public Access" : "SQL Server - Internal VPC Only"
  }

  egress {
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    cidr_blocks = ["0.0.0.0/0"]
    description = "Allow all outbound traffic"
  }

  tags = {
    Project = var.project_name
    Service = "mssql"
  }
}
