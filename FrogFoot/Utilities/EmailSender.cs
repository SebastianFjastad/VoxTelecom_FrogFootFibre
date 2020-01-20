using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Mail;
using FrogFoot.Areas.Home.Models;
using FrogFoot.Entities;
using FrogFoot.Models;
using FrogFoot.Repositories;

namespace FrogFoot.Utilities
{
    public static class EmailSender
    {
        private static OrderRepository orderRepo = new OrderRepository();

        public static void SendEmail(EmailDto email, User user, bool incoming = false)
        {
            var smtp = NewClient();

            string ffEmail = ConfigurationManager.AppSettings["FrogfootEmail"];
            string supportEmail = ConfigurationManager.AppSettings["SupportEmail"];

            //client support email
            if (incoming)
            {
                var body = email.Body + "<br/><br/>From: " + user.FirstName + " " + user.LastName +
                           "<br/><br/> Number: " + user.PhoneNumber;

                MailMessage message = new MailMessage()
                {
                    IsBodyHtml = true,
                    Subject = email.Subject,
                    Body = body,
                    Bcc = { supportEmail }
                };

                message.From = new MailAddress(ffEmail, "Frogfoot Fibre");
                message.To.Add(new MailAddress(user.Email));

                smtp.Send(message);
            }
            else
            {
                try //outgoing email from the portal
                {
                    MailMessage message = new MailMessage()
                    {
                        IsBodyHtml = true,
                        Body = email.Body,
                        Subject = email.Subject
                    };

                    message.From = new MailAddress(ffEmail, "Frogfoot Fibre");
                    message.To.Add(new MailAddress(user.Email));

                    smtp.Send(message);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Exception caught in SendEmail(): {0}", ex);
                }
            }
        }

        public static void SendEmail(EmailDto email)
        {
            var smtp = NewClient();

            string ffEmail = ConfigurationManager.AppSettings["FrogfootEmail"];
            string zoneResultsEmail = ConfigurationManager.AppSettings["ZoneResultsEmail"];

            MailMessage message = new MailMessage()
            {
                IsBodyHtml = true,
                Body = email.Body,
                Subject = email.Subject
            };

            message.From = new MailAddress(ffEmail, "Frogfoot Fibre");
            message.To.Add(new MailAddress(zoneResultsEmail));

            smtp.Send(message);
        }

        public static void SendReminderToISPs()
        {
            bool allowSendReminders = Convert.ToBoolean(ConfigurationManager.AppSettings["RunEmailReminderCron"]);
            if (allowSendReminders)
            {
                var newOrders = orderRepo.GetOrders().Where(o => o.Status == OrderStatus.New);

                //get all the new orders, group them into lists by ISP
                List<List<Order>> groupedOrders = newOrders.GroupBy(o => o.ISPId)
                    .Select(grp => grp.ToList())
                    .ToList();

                //each ISP's new orders
                foreach (var orderGroup in groupedOrders)
                {
                    var newOrdersCount = orderGroup.Count();
                    var isp = orderGroup.First().ISP;
                    SendISPOrderReminderEmail(isp, newOrdersCount);
                }
            }
        }

        public static void SendISPOrderReminderEmail(ISP isp, int newOrders)
        {
            var smtp = NewClient();

            string ffEmail = ConfigurationManager.AppSettings["FrogfootEmail"];

            var body = string.Format("Hi {0} team, <br/><br/>You have {1} new fibre order(s) on the Frogfoot Fibre portal that need attending to.." +
                                     "<br/><br/>Please log on to the Frogfoot Fibre portal <strong><a href=\"" + "http://www.frogfootfibre.com/Account/Login" + "\">here</strong> to tend to the orders." +
                                     "<br/><br/>Regards," +
                                     "<br/><br/>The Frogfoot Fibre team", isp.Name, newOrders);

            string subject = "New orders reminder";

            MailMessage message = new MailMessage()
            {
                IsBodyHtml = true,
                Body = body,
                Subject = subject,
            };

            message.From = new MailAddress(ffEmail, "Frogfoot Fibre");
            message.To.Add(new MailAddress(isp.EmailAddress));

            smtp.Send(message);
        }

        public static void SendISPNewOrderNotification(Order order, ISP isp, User user, string orderUrl)
        {
            if (!isp.ImmediateOrderEmailNotification) return;

            var orderUrlWithOrderId = orderUrl + "/" + order.OrderId;
            var smtp = NewClient();
            var body =
                string.Format(
                    "This is an automated email from the Frogfoot Fibre portal with details of a new order for Fibre Broadband " +
                    "which has been captured on the portal and needs to be actioned by {0}." +
                    "<br/><br/>If this mail is processed by a Service Representative, you can follow the link below, use your Frogfoot Fibre " +
                    "portal credentials to log into the portal and if all is in order and you are ready, place the order for a fibre link with Frogfoot." +
                    "<br/><br/>If this mail is to be processed by your provisioning system, we assume that the order info supplied below will be automatically " +
                    "captured and at a later date a Service Representative will place the order for a fibre link with Frogfoot. " +
                    "<br/><br/><a href=\'" + orderUrlWithOrderId + "\'>Fibre Order</a>" +
                    "<br/><br/><h2>Service Order</h2>" +
                    "<br/><br/><h3>Client Details</h3>" +
                    "Suburb: {1} <br/>First name: {2}<br/> Last name: {3}<br/> Email: {4}<br/> Phone number: {5}<br/> Landline: {6}<br/> Created date: {7}<br/> Latitude: {8}" +
                    "<br/> Longitude: {9}<br/> Address: {10}<br/> ISP Order no: {11}<br/> Status: {12}" +
                    "<h3>Product</h3>" +
                    "<br/> Product name: {13}<br/> Line speed: {14}<br/> Up speed: {15}<br/> Is Capped: {16}" +
                    "<br/> Cap: {17}<br/> Monthly cost: {18}<br/> Setup cost: {19}",
                    isp.Name, user.Location.Name, user.FirstName, user.LastName, user.Email, user.PhoneNumber,
                    user.Landline, order.CreatedDate, user.Latitude, user.Longitude, user.Address, order.ISPOrderNo,
                    order.Status, order.ISPProduct.ProductName, order.ISPProduct.LineSpeed, order.ISPProduct.UpSpeed,
                    order.ISPProduct.IsCapped, order.ISPProduct.Cap, order.ISPProduct.MonthlyCost, order.ISPProduct.SetupCost);

            string subject = "New order notification";

            MailMessage message = new MailMessage()
            {
                IsBodyHtml = true,
                Body = body,
                Subject = subject,
            };

            string ffEmail = ConfigurationManager.AppSettings["FrogfootEmail"];
            message.From = new MailAddress(ffEmail, "Frogfoot Fibre");
            message.To.Add(new MailAddress(isp.EmailAddress));
            smtp.Send(message);
        }

        public static void SendRTNewOrderNotification(int orderId)
        {
            var order = orderRepo.GetOrder(orderId);
            var user = order.Client;

            //if order status == Ordered then send email
            if (order.Status == OrderStatus.Ordered)
            {
                var smtp = NewClient();
                FFProduct ffProduct = order.OrderFFProducts.First(p => p.FFProduct.Type == ProductType.LineSpeed).FFProduct;
                var voiceChannelCount = order.OrderFFProducts.Where(p => p.FFProduct != null && p.FFProduct.Type == ProductType.Quantity);
                var firstVc = order.OrderFFProducts.FirstOrDefault(p => p.FFProduct.Type == ProductType.Quantity);
                var voiceChannelCostMonthly = "0";
                if (firstVc != null)
                {
                    voiceChannelCostMonthly = firstVc.FFProduct.UnitPriceMonthly.ToString();
                }
                var ups = order.OrderFFProducts.FirstOrDefault(p => p.FFProduct.Type == ProductType.Option);
                var upsMonthlyCost = "0";
                if (ups != null)
                {
                    upsMonthlyCost = ups.FFProduct.UnitPriceMonthly.ToString();
                }
                var upsCount = string.IsNullOrEmpty(upsMonthlyCost) ? "0" : "1";

                var body =
                    string.Format(
                        "<h2>New Service Order</h2>" +
                        "<h3>Client Details</h3>" +
                        "Suburb: {1} <br/>First name: {2}<br/> Last name: {3}<br/> Email: {4}<br/> Phone number: {5}<br/> Landline: {6}<br/> Created date: {7}<br/> Latitude: {8}" +
                        "<br/> Longitude: {9}<br/> Address: {10}<br/> ISP Order no: {11}<br/> Client Code: {12}<br/> Status: {13}" +
                        "<h3>ISP Product</h3>" +
                        "ISP: {14}" +
                        "<br/> Product name: {15}<br/> Line speed: {16}<br/> Up speed: {17}<br/> Is Capped: {18}" +
                        "<br/> Cap: {19}<br/> Monthly cost: {20}<br/> Setup cost: {21}" +
                        "<h3>Frogfoot Order</h3>" +
                        "Access Link: {22}<br/> Quantity: 1, Once-off Fee: {23}, Monthly Fee: {24}<br/>" +
                        "32kbps Voice Channel:<br/> Quantity: {25}, Once-off Fee: 0, Monthly Fee: {26}<br/>" +
                        "UPS:<br/> Quantity: {27}, Once-off Fee: 0, Monthly Fee: {28}<br/>",
                        "", user.Location.Name, user.FirstName, user.LastName, user.Email, user.PhoneNumber,
                        user.Landline, order.CreatedDate, user.Latitude, user.Longitude, user.Address, order.ISPOrderNo, order.FFOrderNo,
                        order.Status, order.ISP.Name, order.ISPProduct.ProductName, order.ISPProduct.LineSpeed, order.ISPProduct.UpSpeed,
                        order.ISPProduct.IsCapped, order.ISPProduct.Cap, order.ISPProduct.MonthlyCost, order.ISPProduct.SetupCost, ffProduct.LineSpeed,
                        ffProduct.UnitPriceOnceOff, ffProduct.UnitPriceMonthly, voiceChannelCount.Count(), voiceChannelCostMonthly, upsCount, upsMonthlyCost);

                string subject = "New order notification";

                MailMessage message = new MailMessage()
                {
                    IsBodyHtml = true,
                    Body = body,
                    Subject = subject
                };

                string rtEmail = ConfigurationManager.AppSettings["RTEmail"];
                string ffEmail = ConfigurationManager.AppSettings["FrogfootEmail"];

                message.From = new MailAddress(ffEmail, "Frogfoot Fibre");
                message.To.Add(new MailAddress(rtEmail));
                smtp.Send(message);
            }
        }

        public static void SendRTAcceptedOrderPDF(int orderId)
        {
            var order = orderRepo.GetOrder(orderId);
            if (order.Status != OrderStatus.Accepted) return;

            var smtp = NewClient();
            MailMessage message = new MailMessage {Subject = "Accepted order PDF"};

            var attachment = new object();
            var pdf = order.PDFs.LastOrDefault();
            if (pdf != null)
            {
                attachment = new Attachment(pdf.AssetPath);
            }

            if (attachment is Attachment)
            {
                message.Attachments.Add(attachment as Attachment);
            }

            string rtEmail = ConfigurationManager.AppSettings["RTEmail"];
            string ffEmail = ConfigurationManager.AppSettings["FrogfootEmail"];
            message.From = new MailAddress(ffEmail, "Frogfoot Fibre");
            message.To.Add(new MailAddress(rtEmail));
            smtp.Send(message);
        }

        private static SmtpClient NewClient()
        {
            SmtpClient smtp = new SmtpClient
            {
                Host = "in-v3.mailjet.com",
                Credentials = new NetworkCredential("eee09cb5eb2e9c563e1dd1e48db10eeb", "77f41395dd0f0f66d22cefef6c7f1e49"),
                Port = 587,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                Timeout = 5000
            };
            return smtp;
        }
    }
}