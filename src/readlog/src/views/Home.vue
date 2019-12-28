<template>
  <div class="div-console">
    <div v-for="(item, i) in list" :key="i">
      <div class="div-more-text">
        <span class="div-time">{{item.timestamp}}</span>
        <el-divider direction="vertical"></el-divider>
        <span :class="'div-level '+ item.level">{{item.level}}</span>
        <el-divider direction="vertical"></el-divider>
        <span>{{item.title}}</span>
      </div>
      <div v-for="(line,li) in item.lines" :key="li" class="div-more-text div-tab">{{line}}</div>
      <br />
    </div>
  </div>
</template>

<script>
export default {
  name: "home",
  data() {
    return {
      list: []
    };
  },
  methods: {
    async get() {
      var list = await this.$axios.post(
        "http://localhost:9222/logstash-*/_search",
        {
          from: 0,
          size: 200,
          sort: [
            {
              "@timestamp": {
                order: "DESC"
              }
            }
          ]
        }
      );
      this.list = [
        ...list.data.hits.hits.map(x => {
          var t = new Date(x._source["@timestamp"]);
          return {
            timestamp:
              t.toLocaleDateString() + " " + t.toTimeString().split(" ")[0],
            level: x._source.level.substring(0, 3).toUpperCase()
            // title: x._source.message.split("\r\n")[0].replace(/"/g, ""), TODO:拆分字段赋值
            // lines: x._source.message
            //   .split("\r\n")
            //   .slice(1)
            //   .map(x =>
            //     x
            //       .replace(/\\\"/g, '"')
            //       .replace(/"{/g, "{")
            //       .replace(/"}/g, "}")
            //   )
          };
        })
      ];
      console.log(this.list[0]);
    }
  },
  async mounted() {
    await this.get();
  }
};
</script>

<style>
/* 滚动槽 */
::-webkit-scrollbar {
  width: 6px;
  height: 6px;
}
::-webkit-scrollbar-track {
  border-radius: 3px;
  background: rgba(0, 0, 0, 0.06);
  -webkit-box-shadow: inset 0 0 5px rgba(0, 0, 0, 0.08);
}
/* 滚动条滑块 */
::-webkit-scrollbar-thumb {
  border-radius: 3px;
  background: rgba(0, 0, 0, 0.12);
  -webkit-box-shadow: inset 0 0 10px rgba(0, 0, 0, 0.2);
}

.div-console {
  background-color: #303133;
  padding: 10px 20px;
  color: #fff;
  min-height: 94vh;
  max-height: 94vh;
  font-family: Consolas, Menlo, Monaco, Lucida Console, Liberation Mono,
    DejaVu Sans Mono, Bitstream Vera Sans Mono, Courier New, monospace, serif;
  font-weight: 400;
  font-size: 13px;
  line-height: 1.2rem;
  overflow-y: auto;
}

.div-time {
  color: darkgrey;
}

.div-level {
}

.DEB {
  background-color: #909399;
}

.INF {
  background-color: #67c23a;
}

.ERR {
  background-color: #f56c6c;
}

.WAR {
  background-color: #e6a23c;
}

.div-more-text {
  text-overflow: ellipsis;
  overflow: hidden;
  white-space: pre;
}

.div-tab {
  margin-left: 16px;
}
</style>


