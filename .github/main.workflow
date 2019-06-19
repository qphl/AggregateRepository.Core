workflow "PR Checklist" {
  on = "pull_request"
  resolves = ["Create PR Comment"]
}

action "Create PR Comment" {
  uses = "cognisant/code-review-checklist@master"
  secrets = ["GITHUB_TOKEN"]
}
