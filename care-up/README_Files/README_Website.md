# Issue Logs & Fixes

## Certificate Name was displayed wrongly
- The issue is that the certificate name is wrong for the protocol `Intramuscular Injection(stretch technique)`.
- Instead of `Intramuscular Injection(stretch technique)` it was displayed as `Intramuscular Injection(perpendicular technique)`. This was due to the fact that when the website was created the `protocol names` where created wrongly.
- The protocol names can be found in `Display >> Options` in the `WordPress` website.
- This issue was seen in the `My Account` page of the website where the `Dashboard` is shown.
- To fix this, the `Certificate.php` and `Scores.php` scripts in `wp-content >> themes >> astra-child >> includes >> helpers` are altered. 
- Whenever the database is queried and displayed `if` conditions are made to replace the name for this particular protocol.