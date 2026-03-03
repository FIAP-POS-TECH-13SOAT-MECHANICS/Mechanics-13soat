data "aws_iam_role" "lab_role" {
  name = "LabRole"
}

resource "aws_s3_bucket" "lambda_artifacts" {
  bucket = "${local.prefix}-lambda-artifacts-${data.aws_caller_identity.current.account_id}"
}

resource "aws_s3_object" "auth_zip" {
  bucket = aws_s3_bucket.lambda_artifacts.id
  key    = "auth/placeholder.zip"
  source = "${path.module}/placeholders/lambda-placeholder.zip"
  etag   = filemd5("${path.module}/placeholders/lambda-placeholder.zip")
}

resource "aws_lambda_function" "auth_lambda" {
  function_name = "${local.prefix}-auth"

  s3_bucket = aws_s3_bucket.lambda_artifacts.id
  s3_key    = aws_s3_object.auth_zip.key

  runtime = "dotnet10"
  handler = "Mechanics.Auth.Api"

  role = data.aws_iam_role.lab_role.arn

  memory_size = 256
  timeout     = 10

  environment {
    variables = {
      SecretProviderOptions__PrivateKeySecretName = aws_secretsmanager_secret.jwt_private_key.name
      SecretProviderOptions__DbSecretName         = aws_secretsmanager_secret.db_credentials.name
    }
  }
}

resource "aws_lambda_permission" "api_permission" {
  statement_id  = "AllowAPIGatewayInvoke"
  action        = "lambda:InvokeFunction"
  function_name = aws_lambda_function.auth_lambda.function_name
  principal     = "apigateway.amazonaws.com"
  source_arn    = "${aws_apigatewayv2_api.api_gateway.execution_arn}/*/*"
}
