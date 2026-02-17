using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;

namespace eQuantic.Core.Data.EntityFramework.SqlServer.Tests;

[TestFixture]
public class ExpressionConverterTests
{
    [Test]
    public void ConvertExpression_WithNullArgument_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => 
            ExpressionConverter<TestEntity>.ConvertExpression(null!));
    }

    [Test]
    public void ConvertExpression_WithInvalidExpressionType_ThrowsNotSupportedException()
    {
        // Expressions that are not MemberInit (new Entity { ... }) should fail
        Expression<Func<TestEntity, TestEntity>> invalidExpr = x => x; 
        
        Assert.Throws<NotSupportedException>(() => 
            ExpressionConverter<TestEntity>.ConvertExpression(invalidExpr));
    }

#if NET7_0_OR_GREATER && !NET10_0_OR_GREATER
    [Test]
    public void ConvertExpression_Net7_ReturnsValidExpressionTree()
    {
        // Arrange
        Expression<Func<TestEntity, TestEntity>> updateExpr = x => new TestEntity 
        { 
            Name = "New Name",
            Price = 10.5m 
        };

        // Act
        var result = ExpressionConverter<TestEntity>.ConvertExpression(updateExpr);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.InstanceOf<Expression<Func<SetPropertyCalls<TestEntity>, SetPropertyCalls<TestEntity>>>>());
        
        // Verify if it contains SetProperty calls
        var bodyString = result.Body.ToString();
        Assert.That(bodyString, Contains.Substring("SetProperty"));
        Assert.That(bodyString, Contains.Substring("New Name"));
    }
#endif

#if NET10_0_OR_GREATER
    [Test]
    public void ConvertExpression_Net10_ReturnsCompiledAction()
    {
        // Arrange
        Expression<Func<TestEntity, TestEntity>> updateExpr = x => new TestEntity 
        { 
            Name = "Updated" 
        };

        // Act
        var action = ExpressionConverter<TestEntity>.ConvertExpression(updateExpr);

        // Assert
        Assert.That(action, Is.Not.Null);
        Assert.That(action, Is.InstanceOf<Action<UpdateSettersBuilder<TestEntity>>>());
        
        var builder = new UpdateSettersBuilder<TestEntity>();
        Assert.DoesNotThrow(() => action(builder));
    }
#endif
}