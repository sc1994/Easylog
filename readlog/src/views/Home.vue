<template>
  <div class="div-console">
    <br />
    <div
      v-for="(item, i) in list"
      :key="i"
      style="cursor: pointer;"
      @click="message = item.message;dialogVisible = true;"
    >
      <div class="div-more-text">
        <span class="div-time">{{item.timestamp}}</span>
        <el-divider direction="vertical"></el-divider>
        <span :class="'div-level '+ item.level">{{item.level}}</span>
        <el-divider direction="vertical"></el-divider>
        <span :title="item.title">{{item.title}}</span>
        <div class="div-more-text" v-if="item.exception">Exception: {{item.exception}}</div>
        <div
          class="div-more-text"
          v-if="item.requestHeaders"
        >RequestHeaders: {{item.requestHeaders}}</div>
        <div class="div-more-text" v-if="item.requestBody">RequestBody: {{item.requestBody}}</div>
        <div
          class="div-more-text"
          v-if="item.requestCookies"
        >RequestCookies: {{item.requestCookies}}</div>
        <div
          class="div-more-text"
          v-if="item.responseHeader"
        >responseHeader: {{item.responseHeader}}</div>
        <div class="div-more-text" v-if="item.responseBody">responseBody: {{item.responseBody}}</div>
        <div
          class="div-more-text"
          v-if="item.responseCookies"
        >ResponseCookies: {{item.responseCookies}}</div>
        <div class="div-more-text" v-if="item.filter1">Filter1: {{item.filter1}}</div>
        <div class="div-more-text" v-if="item.filter2">Filter2: {{item.filter2}}</div>
      </div>
      <br />
    </div>
    <el-dialog :visible.sync="dialogVisible" width="80%" :show-close="false" :destroy-on-close="true">
      <div style="white-space:pre;">{{message}}</div>
    </el-dialog>
  </div>
</template>

<script>
import dateFormat from "dateformat";

export default {
  name: "home",
  data() {
    return {
      list: [],
      dialogVisible: false,
      message: ""
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
            message: x._source.message,
            timestamp: dateFormat("yyyy-mm-dd HH:MM:ss"),
            level: x._source.level.substring(0, 3).toUpperCase(),
            title: `<${x._source.fields.ip}> <${x._source.fields.environment}> <${x._source.fields.app}> <${x._source.fields.method}> <${x._source.fields.url}> <${x._source.fields.trace}>`,
            requestHeaders: x._source.fields.requestHeaders,
            exception: x._source.fields.exception,
            requestHeaders: x._source.fields.requestHeaders,
            requestBody: x._source.fields.requestBody,
            requestCookies: x._source.fields.requestCookies,
            responseHeader: x._source.fields.responseHeader,
            responseBody: x._source.fields.responseBody,
            responseCookies: x._source.fields.responseCookies,
            filter1: x._source.fields.filter1,
            filter2: x._source.fields.filter2
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
.div-console {
  background-color: #303133;
  color: #fff;
  padding: 0px 10px;
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
  white-space: nowrap;
}

.div-tab {
  margin-left: 16px;
}
</style>


