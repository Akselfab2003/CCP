# Supporter Invitation Changes - TODO

## ✅ Hvad er lavet:

### 1. **Ny struktur**
- ✅ `InviteSupporter` → Email-baseret invitation (som InviteCustomer)
- ✅ `PromoteSupporterToManager` → Promover eksisterende supporter til manager

### 2. **Backend opdateringer**
- ✅ `IdentityService.API/Endpoints/SupporterEndpoints.cs` → Parameter ændret fra `Guid customerId` til `string email`
- ✅ `IdentityService.Application/Services/Supporter/ISupporterService.cs` → Interface opdateret
- ✅ `IdentityService.Application/Services/Supporter/SupporterService.cs` → Implementation ændret til email-invitation
- ✅ `IdentityService.Sdk/Services/Supporter/ISupporterService.cs` → SDK interface opdateret

### 3. **UI opdateringer**
- ✅ `ui/CCP.UI/Pages/InviteSupporter/` → Simpel email-baseret form
- ✅ `ui/CCP.UI/Pages/PromoteSupporterToManager/` → Ny side med dropdown promovering
- ✅ `ui/CCP.UI/Layout/CCPLayout.razor` → Navigation links tilføjet

---

## ⚠️ VIGTIGT: KIOTA KLIENT SKAL REGENERERES!

### **Hvad mangler:**

Kiota auto-genereret klient (`IdentityService.Sdk.Generated`) skal regenereres fordi API signaturen er ændret:

**Før:**
```
POST /supporter/Invite?customerId={guid}
```

**Nu:**
```
POST /supporter/Invite?email={email}
```

### **Sådan regenererer du Kiota klienten:**

1. **Start IdentityService.API** så OpenAPI spec er tilgængelig
2. **Find OpenAPI URL** (normalt `https://localhost:xxxx/swagger/v1/swagger.json`)
3. **Kør Kiota kommando:**

```powershell
cd services\IdentityService\IdentityService.Sdk

kiota generate `
  -l CSharp `
  -c IdentityServiceClient `
  -n IdentityService.Sdk.Generated `
  -d https://localhost:XXXX/swagger/v1/swagger.json `
  -o ./Generated
```

4. **Opdater SupporterServiceClient.cs** til at bruge `req.QueryParameters.Email`:

```csharp
await _client.Client.Supporter.Invite.PostAsync(req =>
{
    req.QueryParameters.Email = email; // ✅ Dette vil virke efter regenerering
}, cancellationToken: ct);
```

5. **Fjern NotImplementedException** fra `SupporterServiceClient.cs` linje ~30

---

## 🧪 Test plan:

### **InviteSupporter:**
1. Gå til `/InviteSupporters`
2. Indtast email
3. Klik "Send Invitation"
4. Tjek logs for success

### **PromoteSupporterToManager:**
1. Gå til `/PromoteSupporterToManager`
2. Vælg supporter fra dropdown
3. Klik "Promote to Manager"
4. Tjek at supporter forsvinder fra listen
5. Tjek at manager vises i højre side

---

## 📁 Fil struktur:

```
ui/CCP.UI/Pages/
├── InviteCustomer/          → Email invitation til customer
├── InviteSupporter/         → Email invitation til supporter (NY)
└── PromoteSupporterToManager/ → Promover supporter → manager (NY)
```

---

## 🔄 Fremtidig arbejde:

1. **Email Service Integration:**
   - Send faktisk invitation email
   - Gem pending invitations
   - Verification links

2. **PromoteSupporterToManager Backend:**
   - Lav `PromoteToManager` API endpoint
   - Implementer i Application layer
   - Opdater SDK

3. **Manager Service:**
   - `GetAllManagers()` endpoint
   - Vis managers i højre side

---

**Spørgsmål?** Kontakt teamet! 🚀
