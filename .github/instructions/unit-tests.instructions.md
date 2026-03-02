---
description: "Instructions for unit testing the codebase."
applyTo:
  - "src/**/*.cs"
  - "tests/**/*.cs"
---
 
## Unit testing Instructions

- Always use xUnit for unit testing in this codebase.
- Do NOT use Moq library. Instead use NSubstitute for mocking dependencies.
- Do NOT use FluentAssertions. Use the built-in Assert class from xUnit for assertions.
- Follow AAA (Arrange, Act, Assert) pattern for structuring your unit tests.
- Naming convention for test methods should be: `MethodName_StateUnderTest_ExpectedBehavior`. For example: `CalculateTotalPrice_WhenCartIsEmpty_ReturnsZero`.
- Ensure that each unit test is independent and does not rely on the state of other tests.
- Tests must be deterministic and should not produce different results when run multiple times.
- For DateTime / DateTimeOffset mocking, use TimeProvider abstraction to allow for easier testing of time-dependent code.
- Test behavior, not implementation details. Avoid asserting private calls unless it's part of the contract.
- Include negative tests (exceptions/guard clauses) and boundary conditions.
- Do not assert full exception messages; assert exception type and key properties/paramName.
- Always create new testing project for the class that you are testing, unless suitable test project already exists. For example, if you are testing a class named "MyClass" that is inside project: `A.B.ProjectName`, ensure that project "A.B.ProjectName.Tests" exist in `tests/UnitTests/A.B.ProjectName.Tests` directory and inside this project add test named after the class you are testing with `Tests` suffix, for example: `MyClassTests.cs`. If the test project already exist, add the test class inside this. If you needed to create new test project, add to source project this definition:
`<ItemGroup>
   <InternalsVisibleTo Include="$(AssemblyName).Tests" />
   <InternalsVisibleTo Include="DynamicProxyGenAssembly2" />
 </ItemGroup>`
- If you create new test file, ensure it is in the correct namespace, for example: `namespace A.B.ProjectName.Tests` and ensure that it has the correct using statements, for example: `using A.B.ProjectName;` and `using Xunit;`.
- If you create new test file, do not put is in any additional folder.
- If you need to mock `Microsoft.AspNetCore.Authentication.BaseContext<T>` use instead `FakeAuthenticationContext<T>` from `Nerdolando.Bff.TestingUtils` project.
- Do not create any other fake classes, if you need to mock some other class, use NSubstitute to create a substitute for it. If you need to mock some complex behavior, consider creating a helper method inside the test class to set up the substitute.