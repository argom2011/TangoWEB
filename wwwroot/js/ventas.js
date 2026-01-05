let items = [];
let productosData = [];

document.addEventListener("DOMContentLoaded", () => cargarCombos());

function cargarCombos() {
    // Clientes
    fetch("/api/Pedidos/ClientesCombo")
        .then(r => r.json())
        .then(data => {
            let c = document.getElementById("clienteSelect");
            c.innerHTML = "";
            data.forEach(x => {
                let o = document.createElement("option");
                o.value = x.ClienteID;
                o.text = x.Display;
                c.appendChild(o);
            });
        });

    // Productos
    fetch("/api/Pedidos/ProductosCombo")
        .then(r => r.json())
        .then(data => {
            productosData = data; // guardamos precios
            let p = document.getElementById("productoSelect");
            p.innerHTML = "";
            data.forEach(x => {
                let o = document.createElement("option");
                o.value = x.ProductoID;
                o.text = x.Display;
                p.appendChild(o);
            });
        });
}

function agregarItem() {
    let productoSelect = document.getElementById("productoSelect");
    let productoID = productoSelect.value;
    let cantidad = parseInt(document.getElementById("cantidad").value);

    // Buscamos el precio real
    let prod = productosData.find(p => p.ProductoID == productoID);
    let precio = prod ? prod.Precio : 0;

    let item = {
        productoID: productoID,
        nombre: productoSelect.options[productoSelect.selectedIndex].text,
        cantidad: cantidad,
        precioUnitario: precio,
        subtotal: cantidad * precio
    };

    items.push(item);
    renderDetalle();
}

function renderDetalle() {
    let tbody = document.getElementById("detalleBody");
    tbody.innerHTML = "";
    let total = 0;

    items.forEach((i, index) => {
        total += i.subtotal;
        tbody.innerHTML += `
            <tr>
                <td>${i.nombre}</td>
                <td>${i.cantidad}</td>
                <td>${i.precioUnitario.toFixed(2)}</td>
                <td>${i.subtotal.toFixed(2)}</td>
                <td>
                    <button class="btn btn-danger btn-sm" onclick="eliminarItem(${index})">❌</button>
                </td>
            </tr>`;
    });

    document.getElementById("total").innerText = total.toFixed(2);
}

function eliminarItem(index) {
    items.splice(index, 1);
    renderDetalle();
}

function confirmarVenta() {
    let pedido = {
        clienteID: document.getElementById("clienteSelect").value,
        total: parseFloat(document.getElementById("total").innerText),
        items: items
    };

    fetch("/api/Pedidos/ConfirmarVenta", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(pedido)
    })
        .then(r => r.text())
        .then(msg => {
            alert(msg);
            items = [];
            renderDetalle();
        });
}
