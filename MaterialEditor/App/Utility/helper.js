define([], function () {
    return {
        deriveEquipmentClass: function(ctlgDsc, eqpCls)
        {
            var val = '';

            if (eqpCls !== undefined && eqpCls !== '') {
                val = eqpCls;
            }
            else if (ctlgDsc === undefined || ctlgDsc === '') {
                val = eqpCls;
            }
            else {
                var spaceIndex = ctlgDsc.indexOf(' ');
                var commaIndex = ctlgDsc.indexOf(',');

                if (spaceIndex > 0 && commaIndex > 0) {
                    if (commaIndex < spaceIndex) {
                        val = ctlgDsc.slice(0, commaIndex);
                    }
                    else {
                        val = ctlgDsc.slice(0, spaceIndex);
                    }
                }
                else if (spaceIndex > 0) {
                    val = ctlgDsc.slice(0, spaceIndex);
                }
                else if (commaIndex > 0) {
                    val = ctlgDsc.slice(0, commaIndex);
                }
                else
                    val = ctlgDsc;
            }

            return val;
        }
    };
});