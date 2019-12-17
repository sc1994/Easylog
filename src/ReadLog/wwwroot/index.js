new Vue({
    el: '#app',
    data: function () {
        return {
            filtrate: {
                times: [],
                timeSelect: "60",
                loseParent: false,
                keywordSort: false
            },
            switchTime: false,
            classifies: [],
            ips: [],
            tableData: [],
            total: 0,
            dialogVisible: false,
            current: {},
            currentPage: 1,
            pageSize: 50,
            autoRefresh: false,
            timer: {},
            loading: false
        };
    },
    methods: {
        async search(pageIndex) {
            this.currentPage = pageIndex;
            this.loading = true;
            var res = await axios.post("api/search", {
                ...this.filtrate,
                pageIndex,
                pageSize: this.pageSize
            });
            var content = res.data[0];
            this.tableData = content.hits.hits.map(x => {
                return {
                    timestamp: new Date(x._source["@timestamp"]).format("yyyy-MM-dd hh:mm:ss.S"),
                    level: x._source.level,
                    ...x._source.fields,
                    ...x.highlight
                };
            });
            this.total = content.hits.total;
            this.loading = false;
            console.log(res.data);
        },
        showDetail(row, column, event) {
            this.dialogVisible = true;
            this.current = row;
            console.log(row, column, event);
        },
        tableRowClassName({
            row
        }) {
            if (row.level === "Warning") {
                return 'warning-row';
            } else if (row.level === "Error" || row.level == "Fatal") {
                return 'error-row';
            }
            return '';
        },
        async handleCurrentChange(val) {
            await this.search(val);
        },
        async handleSizeChange(val) {
            this.pageSize = val;
            await this.search(1);
        },
        toJsonFormat(str) {
            try {
                return JSON.stringify(JSON.parse(str), null, 2);
            } catch (e) {
                return str;
            }
        },
        cascaderFilterMethod(node, keyword) {
            var arr = node.text.split(" / ");
            var where = arr.filter(x => !!x).map(x => x.toLowerCase());
            var result = where.filter(x => x.indexOf(keyword.toLowerCase()) > -1).length > 0;
            return result;
        },
        toMock() {
            var url = "/mock.html"
            url += "?sub=" + this.current.sub_category
            var req = JSON.stringify(JSON.parse(this.current.msg).Request)
            url += "&req=" + req
            window.open(url);
        }
    },
    watch: {
        switchTime(val) {
            if (val) {
                this.filtrate.timeSelect = "0";
            } else {
                this.filtrate.timeSelect = "60";
                this.filtrate.times = [];
            }
        },
        autoRefresh(val) {
            if (val) {
                this.timer = setInterval(() => {
                    this.search(1);
                }, 1500);
            } else {
                window.clearInterval(this.timer);
            }
        },
        dialogVisible() {
            $("textarea").scrollTop(0);
        }
    },
    async mounted() {
        var res = await axios.get("api/search/aggregation");
        this.classifies = res.data;
        // this.ips = res.data.item2;
        await this.search(1); // todo 测试
    }
});