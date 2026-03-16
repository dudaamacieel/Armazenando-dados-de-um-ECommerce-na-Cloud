document.addEventListener("DOMContentLoaded", function () {
    const hoje = new Date();
    const dataFormatada = hoje.toISOString().split("T")[0];
    document.getElementById("dataVencimento").value = dataFormatada;
});

// URL base da API
const API_GENERATE = "http://localhost:7999/api";
const API_VALIDATE = "http://localhost:7215/api";

async function gerarCodigoBarras() {
    const dataVencimento = document.getElementById("dataVencimento").value;
    const valor = document.getElementById("valor").value;

    if (!dataVencimento || !valor) {
        alert("Por favor, preencha todos os campos.");
        return;
    }

    document.getElementById("loader").style.display = "block";
    document.getElementById("resultContent").style.display = "none";
    document.getElementById("gerarButton").disabled = true;

    try {
        const response = await fetch(`${API_GENERATE}/barcode-generate`, {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify({
                dataVencimento: dataVencimento,
                valor: parseFloat(valor)
            })
        });

        if (!response.ok) {
            throw new Error(`Erro: ${response.status}`);
        }

        const data = await response.json();

        document.getElementById("barcodeImage").src =
            `data:image/png;base64,${data.imagemBase64}`;

        document.getElementById("barcodeText").textContent = data.barcode;

        document.getElementById("resultContent").style.display = "block";

    } catch (error) {
        console.error("Erro ao gerar o código de barras:", error);
        alert("Erro ao gerar o código de barras.");
    } finally {
        document.getElementById("loader").style.display = "none";
        document.getElementById("gerarButton").disabled = false;
    }
}

async function validarCodigo() {
    const barcode = document.getElementById("barcodeText").textContent.trim();
    console.log("Barcode enviado:", barcode);

    if (!barcode) {
        alert("Nenhum código de barras para validar.");
        return;
    }

    try {
        const response = await fetch(`${API_VALIDATE}/barcode-validate`, {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify({
                barcode: barcode
            })
        });

        if (!response.ok) {
            throw new Error(`Erro: ${response.status}`);
        }

        const result = await response.json();

        updateValidationUI(result);

    } catch (error) {
        console.error("Erro ao validar código:", error);
        alert("Erro ao validar o código.");
    }
}

function updateValidationUI(result) {
    const barcodeResult = document.getElementById("barcodeText");

    barcodeResult.classList.remove("barcode-valid", "barcode-invalid");

    if (result.valido === true) {
        barcodeResult.classList.add("barcode-valid");
    } else {
        barcodeResult.classList.add("barcode-invalid");
    }
}