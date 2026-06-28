

# 📋 Backend Discovery Questionnaire

## Skilled Workers Marketplace Platform

**Instructions**

* ✔ Answer every question.
* ✔ Choose the option(s) that fit your vision.
* ✔ If you're unsure, write **"Recommend"**, and I'll choose the best option.

---

# SECTION 1 — Project Vision

### 1.1 What is the purpose of this platform?

```
(✅ ) Find nearby skilled workers

(✅ ) Book home services

(✅) Compare workers by ratings

(✅ ) Manage service requests


```

---

### 1.2 This platform is intended for:

```
(✅ ) One country

```

---

### 1.3 Which countries will the first release support?

```
Egypt [we need to add Arabic Translation]
```

---

### 1.4 Is this a

```

(✅) Portfolio Project

```

---

### 1.5 Main objective (one sentence)

```
digital platform that connects skilled workers such as plumbers, carpenters, electricians, and other technicians with customers who need their services. The platform allows customers to easily find trusted workers based on ratings, reviews, and location, while helping workers reach more clients and manage their services efficiently.
```

---

### 1.6 Do you have existing competitors you want to benchmark against?

```
(✅ ) No
```

---

### 1.7 What is the expected launch timeline?

```
(✅ ) Less than 1 month
```

---

### 1.8 Is there a specific MVP feature set you want to ship first?

```
( ) Recommend
```

---

# SECTION 2 — User Types

Who can register?

```
Customer                 True 
Worker                   True 
Admin                    True 
```

---

### 2.1 Can a single account hold multiple roles?

```
( ) Yes [Customer and Worker cannot be Admins]

```

---

### 2.2 Do workers need admin approval before they can go live?

```
( ) No — Instant activation

```

---

### 2.3 Should banned or suspended users be able to re-register?

```
( ) Admin decides per case
```

---

# SECTION 3 — Worker Types

Which professions should the system support?

Choose all.

```
[all be selectec]
[ ] Plumber

[ ] Electrician

[ ] Carpenter

[ ] Painter

[ ] Mason

[ ] Ceramic Installer

[ ] Plaster Technician

[ ] Construction Technician

[ ] AC Technician

[ ] Refrigerator Technician

[ ] Washing Machine Technician

[ ] TV Technician

[ ] Satellite Technician

[ ] Internet Technician

[ ] CCTV Technician

[ ] Smart Home Technician

[ ] Furniture Assembly

[ ] Furniture Repair

[ ] Locksmith

[ ] Glass Technician

[ ] Aluminum Technician

[ ] Solar Technician

[ ] Cleaning

[ ] Gardening

[ ] Pest Control

[ ] Moving Services

[ ] Mobile Car Mechanic

[all be selectec]
```

---

### Can workers belong to multiple professions?

```
True
```

Example

Ahmed

* Plumbing
* Water Heater
* AC Maintenance

---

### 3.1 Should the system support worker skill levels?

```
( ) Yes — e.g. Beginner / Intermediate / Expert
```

---

### 3.2 Can workers set their own availability days and hours per profession?

```
( ) Yes
```

---

### 3.3 Should workers have a public profile page with a unique URL?

```
( ) Yes
```

---

# SECTION 4 — Categories

Who creates categories?

```
( ) Admin only
```

---

Should categories support unlimited nesting?

Example

```
Home Services

    Plumbing

        Leak Repair

        Water Heater

        Pipe Installation

Electrical

Lighting

Smart Home
```

```
True
```

---

Should categories have

```
[ ] Name

[ ] Icon

[ ] Banner

[ ] Description

[ ] SEO URL

[optional ] Parent Category
```

---

### 4.1 Should categories be sortable/orderable by admin?

```
True
```

---

### 4.2 Should categories support multiple languages?

```
( ) No — Single language [English]

```

---

### 4.3 Should inactive/hidden categories still be accessible via direct URL?

```
( ) No [friendly messages that the categoriee not avilable]
```

---

# SECTION 5 — Worker Profile

Should every worker have

```
[ ] Photo

[ ] Cover Photo

[ ] Biography

[ ] Years of Experience

[ ] Skills


[ ] Portfolio Images

[ ] Portfolio Videos

[ ] Service Areas

[ ] Availability Schedule

[ ] Working Hours

[ ] Hourly Rate

[ ] Starting Price

[ ] Completed Jobs

[ ] Average Rating

```

---

### 5.1 Should workers be able to set a "not accepting new jobs" status temporarily?

```
True 
```

---

### 5.2 Should the platform display a "badge" system for top-rated or verified workers?

```
( ) Yes — e.g. Top Rated [based on Rates]
```

---

### 5.3 Should profile completeness be tracked and shown to the worker?

```
( ) No

```

---

### 5.4 Should workers be able to set a minimum job value they will accept?

```
True 
```

---

# SECTION 6 — Services

Can workers create their own services?

```
True 
```

Example

```
Pipe Repair

Price: 150 EGP

Duration: 45 minutes

Category: Plumbing
```

---

Each service should contain

```
[ ] Title

[ ] Description

[] Price Type [Fixed Price or Hourly rate]

[ ] Price

[ ] Images

[ ] Estimated Duration

[ ] Materials Included

[ ] Available Cities

[ ] Tags
```

---

Pricing Model

```
( ) Both
```

---

### 6.1 Should services support add-ons or extras?

```
( ) No
```

---

### 6.2 Should services support quantity selection by the customer?

```
( ) No

```

---

### 6.3 Should services require admin approval before going live?

```
( ) No — Worker publishes directly
```

---

### 6.4 Should services support promotional/discounted pricing?

```
( ) No

```

---

# SECTION 7 — Search

Customer can search by

```
[ ] Worker Name

[ ] Category

[ ] Service

[ ] Rating

[ ] Distance

[ ] Price

[ ] City

[ ] Area

[ ] Experience

[ ] Available Now
```

---

Sorting

```
[ ] Nearest

[ ] Cheapest

[ ] Highest Rated

[ ] Most Experienced

[ ] Most Jobs Completed

[ ] Newest
```

---

### 7.1 Should search support fuzzy/autocomplete suggestions?

```
( ) No
```

---

### 7.2 Should recently viewed workers or services be saved per user?

```
( ) No
```

---

### 7.3 Should customers be able to save/favorite workers or services?

```
( ) Yes

```

---

### 7.4 Should the platform support a "Recommended for you" personalized section?

```
( ) No
```

---

### 7.5 Should search results respect the customer's preferred language?

```

( ) No -> one language [English]

```

---

# SECTION 8 — Booking

How is a booking created?

```

( ) Worker Accepts Request

```

---

Customer can

```
[ ] Cancel

[ ] Reschedule

[ ] Call

[ ] Track Status
```

---

Worker can

```
[ ] Accept

[ ] Reject

[ ] Reschedule

[ ] Mark On The Way

[ ] Start Job

[ ] Pause Job

[ ] Complete Job

[ ] Cancel Job
```

---

Booking Statuses

Choose what you need.

```
[ ] Pending

[ ] Accepted

[ ] Rejected

[ ] Scheduled

[ ] On The Way

[ ] Started

[ ] Paused

[ ] Completed

[ ] Cancelled

[ ] Expired
```

---

### 8.1 Should bookings support recurring/repeat scheduling?

```
( ) No
```

---

### 8.2 Should customers be able to book multiple workers for the same job?

```

( ) No

```

---

### 8.3 What happens if a worker does not respond within a time limit?

```
( ) Booking expires automatically

```

---

### 8.4 Should bookings have an auto-complete trigger if not confirmed by either party?

```

( ) No 
```

---

### 8.5 Should customers be able to rate/tip the worker directly after job completion?

```
( ) Yes
```

---

### 8.6 Should the system support group/bulk bookings 

```


( ) No

```

---

# SECTION 9 — Payments

Payment methods

```
[ ] Cash

[ ] Credit Card

```

---

Money goes first to

```
( ) Platform

```

---

Platform commission

```
True 
```

---

Commission Type

```
( ) Percentage [1%]

```

---

### 9.1 Should workers have an in-platform wallet to accumulate earnings?

```

( ) No
```

---

### 9.2 How do workers withdraw their earnings?

```

( ) On-demand withdrawal

```

---

### 9.3 Should the platform support coupons and discount codes?

```
( ) No
```

---

### 9.4 Should invoices/receipts be auto-generated and sent to customers?

```

( ) Yes — In-app only

```

---

### 9.5 Should payments support partial refunds?

```
( ) No — Full refund only
```

---

---

# SECTION 10 — Reviews

Customers can

```
[ ] Rate Worker

[ ] Review Worker

```

---

Workers can reply

```
True 
```

---

Can reviews be edited?

```
True 
```

---

Can admin hide reviews?

```
False
```

---

### 10.1 Should reviews be verified (only from completed bookings)?

```
( ) Yes — Only verified bookings

```

---

### 10.2 Should customers be prompted/reminded to leave a review after job completion?

```

( ) No

```

---

### 10.3 Should workers be able to flag/report abusive reviews?

```
( ) No
```

---

### 10.4 Should rating breakdown be shown? (e.g. Punctuality, Quality, Communication)

```

( ) No — Single overall rating

```

---

# SECTION 11 — Chat

Need chat?

```
False
```


---

# SECTION 13 — Maps & Location

Need

```
GPS Location

False
```

---

Worker live location

```
False
```

---

Customer live location

```
 False
```

---




### 13.1 Should the system define service zones or restricted areas?

```
( ) No

```

---

### 13.2 Should distance-based pricing be supported?

```
( ) No
```

---

### 13.3 Should address management allow customers to save multiple addresses?

```

( ) No — One address only

```



---

# SECTION 14 — Admin Dashboard

Admin manages

```
[ ] Users

[ ] Workers

[ ] Categories

[ ] Services

[ ] Bookings

[ ] Payments

[ ] Reviews

[ ] Reports

[ ] CMS Pages

[ ] FAQs

[ ] Analytics

[ ] Roles & Permissions

[ ] App Settings
```

---

### 14.1 Should the admin dashboard include a real-time overview/live map of active jobs?

```
( ) No
```

---

### 14.2 Should the admin be able to manually assign a booking to a specific worker?

```

( ) No

```

---

### 14.3 Should the admin dashboard support exporting reports to Excel/CSV?

```
( ) Yes


```

---

### 14.4 Should the platform support white-label or multi-tenant admin panels?

```
( ) No

```

---

### 14.5 Should the admin dashboard have a configurable KPI/metrics panel?

```
( ) No

```

---

# SECTION 15 — Security

Need

```
[ ] JWT

[ ] Refresh Token

[ ] Email Verification

[ ] Rate Limiting

[ ] Audit Logs

[ ] Soft Delete

[ ] Data Encryption

[ ] Permissions

[ ] Role Management


[ ] Login History

```

---

### 15.1 Should the platform comply with a specific data privacy regulation?

```

( ) None

```

---

### 15.2 Should there be IP-based blocking or geo-restriction?

```


( ) No
```

---




---

### 16.3 Should API responses be versioned to support backward compatibility?

```
( ) Yes — e.g. /api/v1/, /api/v2/


```

---

### 16.4 Should database read replicas be configured for reporting/analytics?

```


( ) No
```

---

# SECTION 17 — Client Applications

Backend will serve

```
[ ] Web Admin Dashboard

[ ] Customer Website

```

---

Need

```
REST API
True

GraphQL
False

SignalR
 False
```

---

### 17.1 Should the API support third-party developer access (open API / partner portal)?

```
( ) Yes

```

---

### 17.2 Should mobile apps support offline mode for workers?

```
( ) No [there is no mobile apps]


```

---

### 17.3 Should the system support deep linking (e.g. share a worker profile via URL that opens the app)?

```
( ) No

```

---

# SECTION 18 — File Storage

Store

```
[ ] Profile Photos

[ ] Portfolio Images

[ ] Portfolio Videos

[ ] Invoices

```

---

Storage

```
( ) Local Storage
```

---

### 18.1 Should uploaded files be scanned for malware/viruses?

```
( ) No
```

---

### 18.2 Should uploaded images be automatically resized/optimized?

```
( ) No
```

---

### 18.3 Should there be a file size/type restriction per upload category?

```
( ) No — Single global limit
```

---

### 18.4 Should sensitive documents (IDs, certificates) have access-controlled URLs with expiry?

```

( ) No

```

---

# SECTION 19 — Technical Preferences

Target Framework

```
( ) .NET 10

```

---

Database

```
( ) SQL Server
```

---

ORM

```
( ) Entity Framework Core
```

---

Authentication

```
( ) ASP.NET Core Identity
```

---

Validation

```
( ) FluentValidation

```

---

Object Mapping

```

( ) Manual Mapping

```

---

Logging

```
( ) Built-in Logging

```

---

Background Jobs

```
( ) Hangfire [if needed]
```

---

Caching

```

( ) None


```

---

Search Engine

```
( ) SQL Search
```

---

Deployment

```

( ) Docker

```

---

### 19.1 Should the project include unit and integration tests from the start?

```
( ) Yes — xUnit + Moq
```

---

### 19.2 Should CI/CD pipelines be configured?

```
( ) Yes — GitHub Actions
```

---

### 19.3 Should API documentation be auto-generated?

```
( ) Yes — Swagger
```

---

### 19.4 Should the project support feature flags for gradual feature rollout?

```

( ) No
```

---

### 19.5 Should error tracking / monitoring be integrated?

```


( ) No

```

---

# SECTION 20 — Development Philosophy

What level of backend do you want to build?

```

( ) Startup MVP Backend

```

---

How do you want the codebase to be structured?

```
( ) Single ASP.NET Core Web API Project (KISS)
```

---

### 20.1 Should the codebase be prepared for a future transition to microservices?

```

( ) No — Keep it simple
```

---

### 20.2 What is the team size that will maintain this codebase?

```

( ) 2–3 developers

```

---

### 20.3 Should the project follow domain-driven design (DDD) principles?

```


( ) No

```

---

### 20.4 Should the API follow strict RESTful conventions or allow pragmatic deviations?

```

( ) Pragmatic REST
```

---

# SECTION 21 — Localization & Internationalization

### 21.1 Should the platform support multiple languages?

```

( ) English only
```

---

### 21.2 Should the platform support RTL (Right-to-Left) layout for Arabic?

```
 False
```

---

### 21.3 Should currency be configurable per country?

```

( ) No — Single currency (EGP)

```

---

### 21.4 Should date and time formats adapt to user locale?

```

( ) No [Egyptian local time]


```

---

# SECTION 22 — Dispute & Support

### 22.1 Should the platform have a built-in dispute resolution system?

```
( ) No 

```

---

### 22.2 Should customers be able to open a support ticket from within the app?

```

( ) No [there is no customer support]

```

---

---

# SECTION 23 — Analytics & Reporting

### 23.1 Which analytics are most important for you?

```
[ ] Total Revenue

[ ] Bookings per Day/Week/Month

[ ] Worker Performance

[ ] Top Categories
```

---

### 23.2 Should workers have their own analytics dashboard?

```

( ) No — Admin only

```

---



---

# SECTION 24 — Promotions & Growth

### 24.1 Should the platform support a referral program?

```


( ) No


```

---

### 24.2 Should the platform support loyalty points or rewards?

```


( ) No

```

---

### 24.3 Should the platform support flash deals or time-limited offers?

```


( ) No
```

---

### 24.4 Should there be a featured/sponsored placement for workers or services?

```

( ) No


```

---