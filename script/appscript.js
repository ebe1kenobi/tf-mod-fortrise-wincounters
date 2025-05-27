/**
 * Return row with same id/date if found
 * Returnn last row with same id id date not found
 * Return error if id not found
 */
function doGet(e) {
  const sheet = SpreadsheetApp.getActiveSpreadsheet().getSheetByName("stats");
  const id = e.parameter.id;
  const date = e.parameter.date;

  if (!id || !date) {
    return ContentService.createTextOutput(JSON.stringify({ error: "Missing id or date" }))
      .setMimeType(ContentService.MimeType.JSON);
  }

  const data = sheet.getDataRange().getValues();
  last = 0;
  for (let i = 1; i < data.length; i++) {
    //return row with same id / date
    if (data[i][0] === id && data[i][1] === date) {
      const value = data[i][2];
      return ContentService.createTextOutput(JSON.stringify({
        id: id,
        date: date,
        value: value
      })).setMimeType(ContentService.MimeType.JSON);
    }
    if (data[i][0] === id) {
      last = i;
    }
  }

  //return last row with same id
  if (last > 0) {
    const value = data[last][2];
    return ContentService.createTextOutput(JSON.stringify({
      id: id,
      date: data[last][1],
      value: value
    })).setMimeType(ContentService.MimeType.JSON);
  }

  return ContentService.createTextOutput(JSON.stringify({ error: "ID + DATE Not found" }))
    .setMimeType(ContentService.MimeType.JSON);
}

function doPost(e) {
  const sheet = SpreadsheetApp.getActiveSpreadsheet().getSheetByName("stats");
  const id = e.parameter.id;
  const date = e.parameter.date;
  let value;

  try {
    value = JSON.parse(e.postData.contents).value;
  } catch (err) {
    return ContentService.createTextOutput(JSON.stringify({ error: "Invalid JSON" }))
      .setMimeType(ContentService.MimeType.JSON);
  }

  if (!id || !date || value === undefined) {
    return ContentService.createTextOutput(JSON.stringify({ error: "Missing id or date or value" }))
      .setMimeType(ContentService.MimeType.JSON);
  }


  const data = sheet.getDataRange().getValues();
  for (let i = 1; i < data.length; i++) {
    if (data[i][0] === id && data[i][1] === date) {
      sheet.getRange(i + 1, 3).setValue(value); // mettre à jour
      return ContentService.createTextOutput(JSON.stringify({ status: "row updated" }))
        .setMimeType(ContentService.MimeType.JSON);
    }
  }

  // Sinon on ajoute la ligne
  //sheet.appendRow([id, date, value]); //TODO check parameter column
  appendStatRow(sheet, id, date, value);

  return ContentService.createTextOutput(JSON.stringify({ status: "new row added"}))
    .setMimeType(ContentService.MimeType.JSON);
}

function appendStatRow(sheet, id, date, value) {
  const lastRow = sheet.getLastRow() + 1;
  sheet.getRange(lastRow, 1).setValue(id);
  sheet.getRange(lastRow, 2).setNumberFormat('@STRING@').setValue(date); // 👈 force texte
  sheet.getRange(lastRow, 3).setValue(value);
}