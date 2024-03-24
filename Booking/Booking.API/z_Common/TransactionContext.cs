namespace CQRSBookingKata.API.Helpers;

public class TransactionContext
{
    protected List<DbContext> Contexts = new();

    private List<IDbContextTransaction>? _transactions;
    private bool _failed;

    public TransactionContext Chain(object previous)
    {
        if (previous is DbContext previousContext)
        {
            Contexts.Add(previousContext);

            return this;
        }

        var t = previous as TransactionContext;

        if (t == default && previous is ITransactionable tt)
        {
            t = tt.AsTransaction();
        }

        if (t == default)
        {
            return this;
        }

        Contexts.AddRange(t.Contexts);

        return this;
    }

    public static TransactionContext operator *(TransactionContext t, object previousContext)
    {
        return t.Chain(previousContext);
    }

    public TransactionContext Execute(Action action)
    {
        _transactions = Contexts
            .Select(context => context.Database)
            .Distinct()
            .Select(database => database.BeginTransaction())
            .ToList();

        try
        {
            action();
        }
        catch (Exception ex)
        {
            _failed = true;

            throw new TransactionException("transaction failed", ex);
        }
        finally
        {
            CloseTransactions();
        }

        return this;
    }

    private void CloseTransactions()
    {
        if (_transactions == default)
        {
            return;
        }

        foreach (var transaction in _transactions)
        {
            if (_failed)
            {
                transaction.Rollback();

                continue;
            }

            transaction.Commit();
        }

        _transactions = default;
        _failed = false;
    }
}

public interface ITransactionable
{
    TransactionContext AsTransaction();
}