resource "aws_apigatewayv2_api" "api_gateway" {
  name          = "${local.prefix}-api"
  protocol_type = "HTTP"
}

# auth routes
resource "aws_apigatewayv2_integration" "lambda_integration" {
  api_id = aws_apigatewayv2_api.api_gateway.id

  integration_type       = "AWS_PROXY"
  integration_uri        = aws_lambda_function.auth_lambda.invoke_arn
  payload_format_version = "2.0"
}

resource "aws_apigatewayv2_route" "login" {
  api_id    = aws_apigatewayv2_api.api_gateway.id
  route_key = "POST /api/auth/login"
  target    = "integrations/${aws_apigatewayv2_integration.lambda_integration.id}"
}

resource "aws_apigatewayv2_route" "refresh" {
  api_id    = aws_apigatewayv2_api.api_gateway.id
  route_key = "POST /api/auth/refresh"
  target    = "integrations/${aws_apigatewayv2_integration.lambda_integration.id}"
}

# catch-all for EKS proxy
resource "aws_apigatewayv2_integration" "eks_integration" {
  api_id = aws_apigatewayv2_api.api_gateway.id

  integration_type   = "HTTP_PROXY"
  integration_method = "ANY"

  integration_uri = "http://${aws_lb.eks_nlb.dns_name}/{proxy}"

  payload_format_version = "1.0"
}

resource "aws_apigatewayv2_route" "eks_proxy" {
  api_id    = aws_apigatewayv2_api.api_gateway.id
  route_key = "ANY /{proxy+}"
  target    = "integrations/${aws_apigatewayv2_integration.eks_integration.id}"
}

resource "aws_apigatewayv2_stage" "default" {
  api_id      = aws_apigatewayv2_api.api_gateway.id
  name        = "$default"
  auto_deploy = true
}
