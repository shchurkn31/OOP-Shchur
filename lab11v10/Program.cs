using System;

abstract class PaymentProcessor
{
    public void Process()
    {
        ValidateCard();
        CheckFunds();
        ChargeAmount();
        SendReceipt();
        LogTransaction();
    }

    protected abstract void ValidateCard();

    protected abstract void CheckFunds();

    protected abstract void ChargeAmount();

    protected virtual void SendReceipt()
    {
        Console.WriteLine("Чек надіслано");
    }

    protected virtual void LogTransaction()
    {
        Console.WriteLine("Транзакцію записано");
    }
}

class CreditCardPayment : PaymentProcessor
{
    protected override void ValidateCard()
    {
        Console.WriteLine("Перевірка кредитної картки");
    }

    protected override void CheckFunds()
    {
        Console.WriteLine("Перевірка коштів на кредитній картці");
    }

    protected override void ChargeAmount()
    {
        Console.WriteLine("Оплата кредитною карткою");
    }
}

class DebitCardPayment : PaymentProcessor
{
    protected override void ValidateCard()
    {
        Console.WriteLine("Перевірка дебетової картки");
    }

    protected override void CheckFunds()
    {
        Console.WriteLine("Перевірка балансу картки");
    }

    protected override void ChargeAmount()
    {
        Console.WriteLine("Оплата дебетовою карткою");
    }
}

class WalletPayment : PaymentProcessor
{
    protected override void ValidateCard()
    {
        Console.WriteLine("Перевірка електронного гаманця");
    }

    protected override void CheckFunds()
    {
        Console.WriteLine("Перевірка балансу гаманця");
    }

    protected override void ChargeAmount()
    {
        Console.WriteLine("Оплата електронним гаманцем");
    }
}

class Program
{
    static void Main()
    {
        PaymentProcessor payment = new CreditCardPayment();
        payment.Process();

        Console.WriteLine();

        payment = new DebitCardPayment();
        payment.Process();

        Console.WriteLine();

        payment = new WalletPayment();
        payment.Process();
    }
}