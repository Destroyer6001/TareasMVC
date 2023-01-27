﻿function manejarClickAgregarPaso() {

    const indice = tareaEditarVM.pasos().findIndex(p => p.esNuevo());
    if (indice !== -1)
    {
        return;
    }
    tareaEditarVM.pasos.push(new pasoViewModel({
        modoEdicion = true,
        realizado = false,
    }));

    $("[name =txtPasoDescripcion]:visible").focus();

}

function manejarClickCancelarPaso(paso) {
    if (paso.esNuevo()) {
        tareaEditarVM.pasos.pop();
    } else {
        paso.modoEdicion(false);
        paso.descripcion(paso, descripcionAnterior);
    }

}

async function InsertarPaso(paso, data, idTarea)
{
    const respuesta = await fetch(`${urlPasos}/${idTarea}`, {
        body: data,
        method: "POST",
        headers: {
            'Content-Type':'application/json'
        }
    });

    if (respuesta.ok) {
        const json = await respuesta.json();
        paso.id(json.id);

        const tarea = ObtenerTareaEnEdicion();
        tarea.pasosTotal(tarea.pasosTotal() + 1);

        if (paso.realizado) {
            tarea.pasosRealizados(tarea.pasosRealizados() + 1);
        }
    }
    else {
        ManejarErrorApi(respuesta);
    }

}

async function manejarClickSalvarPaso(paso) {
    paso.modoEdicion(false);
    const esNuevo = paso.esNuevo();
    const idTarea = tareaEditarVM.id;
    const data = obtenerCuerpoPeticionPaso(paso);

    const descripcion = paso.descripcion();
    if (!descripcion) {
        paso.descripcion(paso.descripcionAnterior);

        if (esNuevo) {
            tareaEditarVM.pasos.pop();
        }
    }
    if (esNuevo) {
        InsertarPaso(paso, data, idTarea);
    }
    else {
        actualizarPaso(data, paso.id());
    }
}

function obtenerCuerpoPeticionPaso(paso) {
    return JSON.stringify({
        descripcion: paso.descripcion(),
        realizado: paso.realizado()
    });
}

function manejarClickDescripcionPaso(paso) {
    paso.modoEdicion(true);
    paso.descripcionAnterior = paso.descripcion();
    $("[name = txtPasoDescripcion]: visible").focus();
}


async function actualizarPaso(data, id) {
    const respuesta = await fetch(`${urlPasos}/${id}`, {
        body: data,
        method: "PUT",
        headers: {
            'Content-Type': 'application/json'
        }
    });

    if (!puesta.ok) {
        ManejarErrorApi(respuesta);
    }
}

function manejarClickCheckboxPaso(paso) {

    if (paso.esNuevo()) {
        return true;
    }

    const data = obtenerCuerpoPeticionPaso(paso);
    actualizarPaso(data, paso.id());
    const tarea = ObtenerTareaEnEdicion();
    let pasosRealizadosActual = tarea.pasosRealizados();

    if (paso.realizado()) {
        pasosRealizadosActual++;
    } else {
        pasosRealizadosActual--;
    }

    tarea.pasosRealizados(pasosRealizadosActual);

    return true;
}

function manejarClickBorrarPaso(paso) {
    modalEditarTareaBootstrap.hide();
    confirmarAccion({
        callbackAceptar: () => {
            modalEditarTareaBootstrap.show();
        },
        callbackCancela: () => {
            modalEditarTareaBootstrap.show();
        },
        titulo: `¿Desea borrar este paso?`
    })
}
async function borrarPaso(paso) {
    const respuesta = await fetch(`${urlPasos}/${paso.id()}`, {
        method: 'DELETE'
    });

    if (!respuesta.ok) {
        ManejarErrorApi(respuesta);
        return;
    }

    tareaEditarVM.pasos.remove(function (item) {return item.id() == paso.id() })
}
async function actualizarOrdenPasos() {
    const ids = obtenerIdsPasos();
    await enviarPasosAlBackend(ids);

    const arregloOrganizado = tareaEditarVM.pasos.sorted(function (a, b) {
        return ids.indexOf(a.id().toString()) - ids.indexOf(b.id().toString());
    })

    tareaEditarVM.pasos(arregloOrganizado);
}

function obtenerIdsPasos() {
    const ids = $("[name=chbpaso]").map(function () {
        return $(this).attr('data-id')
    }).get();
    return ids;
}

$(function () {
    $("#reordenable-paso").sortable({
        axis: y,
        stop: async function () {
            await actualizarOrdenPasos();
        }
    })
})

async function enviarPasosAlBackend(ids) {
    var data = JSON.stringify(ids);
    await fetch(`${urlPasos}/ordenar/${tareaEditarVM.id}`, {
        method: 'POST',
        body: data,
        headers: {
                'Content-Type': 'application/json'
        }
    });
}
