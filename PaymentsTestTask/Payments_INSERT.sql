CREATE TRIGGER Payments_INSERT
ON Payments
AFTER INSERT
AS
DECLARE @ordID int, 
		@arrID int,
		@amountPaid float,
		@arrivalRwmains float,
		@totalPaymantAmount float,
		@paymentsInsetedID int
set @paymentsInsetedID =(select i.id 
					from inserted i 
					inner join Payments pay 
					on pay.id =i.id)-- Получаем id добавленного платежа из системной таблицы Inserted
set @ordID = (select Order_id from Payments where id = @paymentsInsetedID) -- id заказа
set @arrID = (select Arrival_id from Payments where id = @paymentsInsetedID) -- id прихода денег
set @amountPaid = (select Amount_Paid from Orders where id = @ordID) -- задолженность  по заказу
set @arrivalRwmains = (select Remains from Arrival where id = @arrID) -- сумма с прихода денег  
set @totalPaymantAmount = (select Total_Order_Amount from Orders where id = @ordID) -- общая сумма заказа

-- Проверяем нужно ли проводить вычисления  для расчета остатка прихода
if @arrivalRwmains > 0
	begin
		if (@amountPaid + @arrivalRwmains) < @totalPaymantAmount
			begin
				update Orders
					set Amount_Paid = @amountPaid + @arrivalRwmains
					where id = @ordID
				update Arrival
					set Remains = 0
					where id = @arrID
				update Payments
					set Payment_Amount = @arrivalRwmains
					where id = @paymentsInsetedID
			end
		else
			begin
				update Orders
					set Amount_Paid = @totalPaymantAmount
					where id = @ordID
				update Arrival
					set Remains = @arrivalRwmains - (@totalPaymantAmount - @amountPaid)
					where id = @arrID
				update Payments
					set Payment_Amount = @totalPaymantAmount - @amountPaid
					where id = @paymentsInsetedID
			end
	end
	 

	--DROP TRIGGER Payments_INSERT