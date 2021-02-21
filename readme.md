## 0. Basic Setup and Info

Email Verification Has been turned off, but Password Reset Works

### Running Application
```bash
$ rm -rf Migrations/ LocalDatabase.db

$ dotnet ef migrations add Initial --context SqliteDataContext

$ dotnet run
```

### Swagger Docs
[http://localhost:4000/swagger/](http://localhost:4000/swagger/)

Swagger Documentation along with `OpenAPI 3.1` Config

### Authentication and Context
[http://localhost:4000/users/authenticate](http://localhost:4000/users/authenticate)

^ returns a `Token`, based on credentials passed, and use that `Token` as `Bearer Token`.

## 1. Registering Admin

[http://localhost:4000/users/register](http://localhost:4000/users/register)

```json
{
  "name": "Admin",
  "email": "admin@example.com",
  "address": "string",
  "city": "string",
  "state": "string",
  "phone": "string",
  "password": "pass12"
}
```

## 2. Registering 3 ParkingManagers

[http://localhost:4000/users/register](http://localhost:4000/users/register)

```json
{
  "name": "P M1",
  "email": "pm1@example.com",
  "address": "string",
  "city": "string",
  "state": "string",
  "phone": "string",
  "password": "pass12",
  "role": "ParkingManager"
}

{
  "name": "P M2",
  "email": "pm2@example.com",
  "address": "string",
  "city": "string",
  "state": "string",
  "phone": "string",
  "password": "pass12",
  "role": "ParkingManager"
}

{
  "name": "P M3",
  "email": "pm3@example.com",
  "address": "string",
  "city": "string",
  "state": "string",
  "phone": "string",
  "password": "pass12",
  "role": "ParkingManager"
}
```
## 3. Creating 3 Garages

[http://localhost:4000/garages/create](http://localhost:4000/garages/create)

```json
{
  "name": "Garage 1-3",
  "address": "string",
  "state": "string",
  "phone": "string",
  "hasCleaningService": true,
  "parkingRate": "75",
  "cleaningRate": "100"
}

{
  "name": "Garage 2-2",
  "address": "string",
  "state": "string",
  "phone": "string",
  "hasCleaningService": false,
  "parkingRate": "100",
  "cleaningRate": "100"
}

{
  "name": "Garage 3-1",
  "address": "string",
  "state": "string",
  "phone": "string",
  "hasCleaningService": false,
  "parkingRate": "50",
  "cleaningRate": "100"
}
```

## 4. Registering 3 AllocationManagers

[http://localhost:4000/users/register](http://localhost:4000/users/register)

garageId: 1
```json
{
  "name": "A M1",
  "email": "am1@example.com",
  "address": "string",
  "city": "string",
  "state": "string",
  "phone": "string",
  "password": "pass12",
  "role": "AllocationManager"
}
```

garageId: 2
```json
{
  "name": "A M2",
  "email": "am2@example.com",
  "address": "string",
  "city": "string",
  "state": "string",
  "phone": "string",
  "password": "pass12",
  "role": "AllocationManager"
}
```

garageId: 3
```json
{
  "name": "A M3",
  "email": "am3@example.com",
  "address": "string",
  "city": "string",
  "state": "string",
  "phone": "string",
  "password": "pass12",
  "role": "AllocationManager"
}
```

## 5. Creating 3 Spaces

[http://localhost:4000/spaces/create](http://localhost:4000/spaces/create)

{
  "code": "101",
  "totalCapacity": "2",
  "garageId": 1
}

{
  "code": "101",
  "totalCapacity": "2",
  "garageId": 2
}

{
  "code": "201",
  "totalCapacity": "2",
  "garageId": 2
}

{
  "code": "101",
  "totalCapacity": "2",
  "garageId": 3
}

## 6. Registering 3 Users

[http://localhost:4000/users/register](http://localhost:4000/users/register)

```json
{
  "name": "User 1",
  "email": "user1@example.com",
  "address": "string",
  "city": "string",
  "state": "string",
  "phone": "string",
  "password": "pass12"
}

{
  "name": "User 2",
  "email": "user2@example.com",
  "address": "string",
  "city": "string",
  "state": "string",
  "phone": "string",
  "password": "pass12"
}

{
  "name": "User 3",
  "email": "user3@example.com",
  "address": "string",
  "city": "string",
  "state": "string",
  "phone": "string",
  "password": "pass12"
}
```

## 7. Booking Parking for User 1 in Space 1 of Garage 1

[http://localhost:4000/parkings/book](http://localhost:4000/parkings/book)

```json
{
  "garageId": 1,
  "spaceId": 1,
  "vehicleNumber": "GJ User 1",
  "driverName": "User 1",
  "withCleaningService": true
}
```

## 8. Checkin Parking for User 1
[http://localhost:4000/parkings/checkin](http://localhost:4000/parkings/checkin)

No Data to be passed

## 9. Checkout Parking for User 1
[http://localhost:4000/parkings/checkout](http://localhost:4000/parkings/checkout)

No Data to be passed

## 10. Get Parking Receipt for User 1
[http://localhost:4000/parkings/receipt](http://localhost:4000/parkings/receipt)

No Data to be passed