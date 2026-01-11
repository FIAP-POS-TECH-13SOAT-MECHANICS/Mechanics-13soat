resource "helm_release" "metrics_server" {
  name      = "metrics-server"
  namespace = "kube-system"

  repository = "https://kubernetes-sigs.github.io/metrics-server/"
  chart      = "metrics-server"

  set = [
    {
      name  = "arg[0]"
      value = "--kubelet-insecure-tls"
    },
    {
      name  = "arg[1]"
      value = "--kubelet-preferred-address-types=InternalIP"
    }
  ]
}

resource "helm_release" "ingress_nginx" {
  name      = "ingress-nginx"
  namespace = "ingress-nginx"

  create_namespace = true

  repository = "https://kubernetes.github.io/ingress-nginx"
  chart      = "ingress-nginx"
}

resource "helm_release" "mailpit" {
  name      = "mailpit"
  namespace = "default"

  repository = "https://jouve.github.io/charts"
  chart      = "mailpit"

  set = [{
    name  = "mailpit.smtp.authFile.enabled"
    value = true
  }]

  set_sensitive = [{
    name  = "mailpit.smtp.authFile.htpasswd"
    value = "${random_string.email_smtp_user.result}@${var.email_domain}:${random_string.email_smtp_password.result}"
  }]
}

resource "helm_release" "external_secrets" {
  name      = "external-secrets"
  namespace = "external-secrets"

  create_namespace = true

  repository = "https://charts.external-secrets.io"
  chart      = "external-secrets"

  wait_for_jobs = true
}
