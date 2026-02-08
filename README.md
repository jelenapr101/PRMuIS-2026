# PRMuIS 2026 – Klasična kriptografija

Projekat predstavlja implementaciju klijent–server modela komunikacije uz
primenu algoritama klasične kriptografije, realizovanu u okviru predmeta
**Primena računarskih mreža u infrastrukturnim sistemima**.

## Opis sistema

Sistem se sastoji od:
- serverske aplikacije koja omogućava komunikaciju sa više klijenata,
- klijentske aplikacije koja šalje i prima poruke,
- zajedničkog domena sa implementacijama kriptografskih algoritama.

Komunikacija između klijenta i servera može se ostvariti korišćenjem **TCP** ili
**UDP** protokola, u zavisnosti od izbora korisnika pri pokretanju aplikacije.

## Implementirani algoritmi klasične kriptografije

U okviru projekta implementirani su sledeći algoritmi:
- Homofono šifrovanje  
- Šifrovanje transpozicijom matrica  
- Vižnerov algoritam  

Algoritam i ključ se biraju na strani klijenta i prosleđuju serveru, koji zatim
koristi iste parametre za dešifrovanje i slanje odgovora.

## Funkcionalnosti sistema

- izbor protokola komunikacije (TCP / UDP),
- istovremeni rad sa više klijenata (neblokirajuća komunikacija),
- čuvanje informacija o klijentima i načinu komunikacije,
- slanje i prijem šifrovanih poruka,
- dešifrovanje poruka na serveru i šifrovanje odgovora.

## Struktura projekta

- **Client** – klijentska aplikacija  
- **Server** – serverska aplikacija  
- **Domain** – zajedničke klase i implementacije algoritama

## Mrežni protokoli

- **TCP** - pouzdan, konekcijski orijentisan, garantuje redoslijed i isporuku, koristi se za request/response komunikaciju 

- **UDP** - nepouzdan, bez konekcije,brzi , poruke se salju kao datagrami, odgovor je opcionalan 

## Napomena

Projekat se razvija postepeno, u skladu sa zadatim specifikacijama i fazama
implementacije.


## Autori:

- **Višnja Maksimović PR 107/2021**
      
- **Jelena Cvjetinović PR 101/2021**
