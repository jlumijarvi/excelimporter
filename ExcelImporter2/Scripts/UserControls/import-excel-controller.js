﻿// import-excel-controller.js

importExcelController = function () {

    var initialized = false;
    var previewData = null;
    var importExcelPage = null;
    var fileId = null;
    var fileUpload = null;
    var alertPanel = null;
    var mappingPanel = null;
    var previewPanel = null;
    var modalDialog = null;

    function clearAll() {
        mappingPanel.fadeOut();
        previewPanel.fadeOut();
    };

    function ajaxHelper(uri, method, data, trigger) {
        if ($(trigger).hasClass('btn')) {
            $(trigger).button('loading');
        }
        $('#modal-progress').modal('show');
        return $.ajax({
            type: method,
            url: uri,
            dataType: 'json',
            contentType: 'application/json',
            data: data ? JSON.stringify(data) : null
        }).error(function (jqXHR, textStatus, errorThrown) {
            clearAll();
            alert('danger', 'Something went wrong. Status: ' + textStatus + '. Error: ' + errorThrown);
        }).complete(function () {
            $('#modal-progress').modal('hide');
            $(trigger).button('reset');
        })
    };

    function populateColumnSelectionDropDown(el) {
        if (_.isEmpty(el.val()))
            return;
        var ddlColumns = el.parent().parent().find('[name$=myDropDownListColumns]');
        var uri = decodeURI('/api/columns/' + el.val());
        $.getJSON(uri).done(function (data) {
            ddlColumns.find('option').remove();
            ddlColumns.append($('<option>').val('').html(''));
            $.each(data, function (i, item) {
                var html = item.name;
                if (item.key) {
                    html = '[key] ' + html;
                }
                ddlColumns.append($('<option>').val(item.name).html(html));
            });
        });
    };

    function alert(status, text) {
        alertPanel.removeAttr('class').addClass('alert alert-' + status + ' fade in').append(text).show();
    }

    return {
        init: function (page) {

            if (initialized) {
                return;
            }

            importExcelPage = page;

            fileId = $(importExcelPage).find('[id$=FileId]').val();
            fileUpload = $(importExcelPage).find('.file-upload');
            alertPanel = $(importExcelPage).find('#alertPane')
            mappingPanel = $(importExcelPage).find('[id$=MappingPanel]');
            previewPanel = $(importExcelPage).find('#previewPanel');
            modalDialog = $(importExcelPage).find('.modal-dialog');

            if (_.isEmpty(fileId)) {
                clearAll();
            }
            else {
                mappingPanel.fadeIn();
                $(document).scrollTop(mappingPanel.offset().top);
            }

            fileUpload.find('.selector').click(function () {
                $(this).siblings('[type=file]').click();
            });

            fileUpload.find('[type=file]').change(function () {
                var self = $(this);
                if (_.isEmpty(self.val())) {
                    self.siblings('.form-control').val('');
                    clearAll();
                    return;
                }
                self.siblings('.form-control').val(self.val().replace(/C:\\fakepath\\/i, ''));
                $(importExcelPage).find('.selector').button('loading');
                $('#modal-progress').modal('show');
                fileUpload.find('[id$=UploadButton]').click();
            });

            modalDialog.parent().on('hide.bs.modal', function () {
                $(this).find('.modal-body').scrollTop(0);
            });

            $(importExcelPage).find('[name$=myDropDownListTables]').change(function () {
                populateColumnSelectionDropDown($(this));
            });

            $(importExcelPage).find('#startPreviewButton, #commitChanges').click(function () {
                var self = $(this);

                var isPreview = (this.id === 'startPreviewButton');
                var uri = decodeURI('/api/import/' + fileId + (isPreview ? '?preview=true' : ''));
                var postData = [];
                $(importExcelPage).find('[name$=myDropDownListTables]').each(function () {
                    var el = $(this);
                    if (_.isEmpty(el.val()))
                        return;
                    var label = el.parent().parent().find('[id*=HeaderLabel]');
                    var ddlColumns = el.parent().parent().find('[name$=myDropDownListColumns]');
                    var newItem = {
                        header: label.text(),
                        table: el.val(),
                        column: ddlColumns.val()
                    };
                    postData.push(newItem);
                });

                ajaxHelper(uri, 'POST', postData, self).success(function (data) {
                    previewData = data;
                    if (isPreview) {
                        var previewChanges = $(importExcelPage).find('#previewChanges');

                        previewChanges.html('');
                        $.each(data, function (i, item) {
                            var items = $('<li>').text('New items: ').
                                append($('<a>').attr('href', '#').attr('data-toggle', 'modal').attr('data-target', '#previewItems').
                                attr('data-table', item.name).attr('data-table-type', 'added').append($('<span>').addClass('badge').text(item.addedCount)));
                            items.append($('<li>').text('Modified items: ').
                                append($('<a>').attr('href', '#').attr('data-toggle', 'modal').attr('data-target', '#previewItems').
                                attr('data-table', item.name).attr('data-table-type', 'modified').append($('<span>').addClass('badge').text(item.modifiedCount))));
                            previewChanges.append(item.name).append($('<ul>').append(items));
                        });

                        previewChanges.find('a').click(function () {
                            var self = $(this);

                            var previewItems = $(importExcelPage).find('#previewItems');
                            var table = previewItems.find('table');
                            table.html('');
                            var modalTitle = previewItems.find('.modal-title');
                            modalTitle.text('');

                            var tablePreviewData = _.where(previewData, { name: self.data('table') })[0];
                            var columns = tablePreviewData.columns;
                            var objects = [];
                            if (self.data('table-type') === 'added') {
                                objects = tablePreviewData.added;
                            } else if (self.data('table-type') === 'modified') {
                                objects = tablePreviewData.modified;
                            }

                            modalTitle.text(self.data('table'));

                            var header = $('<thead>');
                            var headerRow = $('<tr>');

                            $.each(columns, function (i, column) {
                                headerRow.append($('<th>').text(column));
                            });
                            header.append(headerRow);
                            table.append(header);

                            var body = $('<tbody>');
                            $.each(objects, function (i, object) {
                                var newRow = $('<tr>');
                                $.each(object, function (ii, value) {
                                    newRow.append($('<td>').text(value));
                                });
                                body.append(newRow);
                            });
                            table.append(body);
                        });

                        var previewPanel = $('#previewPanel');
                        previewPanel.fadeIn();
                        $(document).scrollTop(previewPanel.offset().top);
                    }
                    else {
                        fileUpload.find('[type=file]').val('').change();
                        alertPanel.removeAttr('class').addClass('alert alert-success fade in').append('Data imported succesfully.').show();
                    }
                });
            });

            $(importExcelPage).find('#confirmIgnoreChanges').popover({
                content: function () {
                    return $($(this).data('popover-content')).html();
                }
            }).on('shown.bs.popover', function () {
                $(importExcelPage).find('button[id=ignoreChanges]').click(function () {
                    var uri = decodeURI('/api/import/' + fileId);
                    ajaxHelper(uri, 'DELETE', null, $(this)).success(function () {
                        fileUpload.find('[type=file]').val('').change();
                        alertPanel.removeAttr('class').addClass('alert alert-info fade in').append('Import cancelled.').show();
                    }).fail(function (data) {
                        fileUpload.find('[type=file]').val('').change();
                        alertPanel.removeAttr('class').addClass('alert alert-info fade in').append('Import cancelled.').show();
                    });
                });
            });
        }
    };
}();
