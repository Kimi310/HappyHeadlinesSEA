# FMEA Analysis – CI/CD Pipeline for HappyHeadlines

**Non-functional requirement:** All microservices in HappyHeadlines are integrated and delivered automatically.

| # | Failure Mode | Effect | Severity (1-10) | Cause | Occurrence (1-10) | Current Controls | Detection (1-10) | RPN | Recommended Action |
|---|---|---|---|---|---|---|---|---|---|
| 1 | Pipeline fails to trigger on push to main | New code is not built or deployed; stale version remains in production | 8 | Misconfigured workflow trigger or GitHub outage | 3 | GitHub Actions status checks, branch protection rules | 3 | 72 | Add branch protection requiring status checks to pass; monitor GitHub status page |
| 2 | Docker build fails for one or more services | Service is not updated; partial deployment | 7 | Breaking code change, missing dependency, Dockerfile error | 5 | Build step logs, fail-fast strategy in matrix build | 2 | 70 | Run `dotnet build` in CI before Docker build; use multi-stage build cache |
| 3 | Unit/integration tests fail silently | Defective code is deployed to production | 9 | Tests not included in pipeline or test step misconfigured | 3 | Test step with `dotnet test`; pipeline fails on non-zero exit code | 3 | 81 | Enforce test step as required check; add code coverage threshold |
| 4 | Container image push to registry fails | Deployment step has no image to pull | 8 | Registry authentication expired, network issue, quota exceeded | 3 | Retry logic, GitHub Actions secrets rotation | 4 | 96 | Use OIDC for registry auth; set up alerts on push failures |
| 5 | Deployment to target environment fails | Users still see old version; potential downtime | 8 | Incorrect environment variables, infrastructure misconfiguration | 4 | Deployment logs, health-check endpoints (`/health`) | 3 | 96 | Implement rolling deployment; add post-deploy health verification step |
| 6 | Secrets (DB passwords, API keys) leaked in logs | Security breach, data compromise | 10 | Secrets printed in build output or committed to repo | 2 | GitHub Actions secret masking, `.gitignore` | 4 | 80 | Use GitHub environment secrets; run secret scanning; never echo secrets |
| 7 | Pipeline takes too long (>15 min) | Developer productivity loss, delayed delivery | 5 | No caching, sequential builds for 10 services | 6 | Matrix/parallel builds, Docker layer caching | 3 | 90 | Use build matrix for parallel service builds; enable Docker BuildKit cache |
| 8 | Incompatible service versions deployed together | Runtime errors, broken inter-service communication | 9 | Services deployed independently without integration test | 4 | Docker Compose integration test step | 5 | 180 | Add integration smoke test after all images are built; use docker-compose in CI |
| 9 | Rollback not possible after bad deploy | Extended downtime with broken version | 8 | No image tagging strategy; latest tag overwritten | 4 | Tag images with Git SHA | 4 | 128 | Tag images with both `latest` and Git SHA; document rollback procedure |
| 10 | GitHub Actions runner unavailable | Pipeline queued indefinitely | 6 | GitHub-hosted runner capacity limits | 2 | GitHub status monitoring | 5 | 60 | Consider self-hosted runner as fallback |

## Risk Priority Summary

| Risk Level | RPN Range | Items |
|---|---|---|
| **High** | ≥ 100 | #8 (180), #9 (128) |
| **Medium** | 70–99 | #4 (96), #5 (96), #7 (90), #6 (80), #3 (81), #1 (72), #2 (70) |
| **Low** | < 70 | #10 (60) |

**Top priority:** Ensure integration testing across services (#8) and implement proper image tagging for rollback (#9).

