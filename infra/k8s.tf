data "aws_iam_roles" "lab_eks_cluster_roles" {
  name_regex = ".*LabEksClusterRole.*"
}

data "aws_iam_role" "lab_cluster_role" {
  name = one(data.aws_iam_roles.lab_eks_cluster_roles.names)
}

data "aws_iam_roles" "lab_eks_node_roles" {
  name_regex = ".*LabEksNodeRole.*"
}

data "aws_iam_role" "lab_node_role" {
  name = one(data.aws_iam_roles.lab_eks_node_roles.names)
}

resource "aws_eks_cluster" "cluster" {
  name     = "${local.prefix}-cluster"
  role_arn = data.aws_iam_role.lab_cluster_role.arn

  access_config {
    authentication_mode                         = "API_AND_CONFIG_MAP"
    bootstrap_cluster_creator_admin_permissions = true
  }

  bootstrap_self_managed_addons = false

  compute_config {
    enabled       = true
    node_pools    = ["general-purpose", "system"]
    node_role_arn = data.aws_iam_role.lab_node_role.arn
  }

  kubernetes_network_config {
    elastic_load_balancing {
      enabled = true
    }
  }

  storage_config {
    block_storage {
      enabled = true
    }
  }

  vpc_config {
    subnet_ids = concat(
      aws_subnet.private.*.id,
      aws_subnet.public.*.id,
    )

    endpoint_private_access = true
    endpoint_public_access  = local.public
  }

  upgrade_policy {
    support_type = "STANDARD"
  }
}
