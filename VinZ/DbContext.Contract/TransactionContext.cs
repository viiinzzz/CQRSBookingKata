/*
 * Copyright (C) 2024 Vincent Fontaine
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

namespace VinZ.Common;

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

            throw new TransactionException($"transaction failed: {ex.Message}", ex);
        }
        finally
        {
            CloseTransactions();
        }

        return this;
    }

    public TransactionContext ExecuteExclusive(Action action, object theLock, int acquireTimeoutMilliseconds = 30_000)
    {
        var acquiredLock = false;

        try
        {
            acquiredLock = Monitor.TryEnter(theLock, acquireTimeoutMilliseconds);

            if (acquiredLock)
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

                    throw new TransactionException($"transaction failed: {(ex.InnerException ?? ex).Message}", ex.InnerException );
                }
                finally
                {
                    CloseTransactions();
                }
            }
            else
            {
                throw new TransactionException(
                    $"transaction failed: exclusive lock not acquired after {acquireTimeoutMilliseconds}ms");
            }
        }
        finally
        {
            if (acquiredLock)
            {
                Monitor.Exit(theLock);
            }
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