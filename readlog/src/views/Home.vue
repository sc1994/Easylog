<template>
  <div
    class="div-console"
    v-infinite-scroll="search"
    style="overflow:auto"
    :infinite-scroll-distance="50"
  >
    <div v-for="(item, i) in list" :key="i" @click="detail(item)">
      <div class="div-more-text">
        <span class="div-time">{{item.timestamp}}</span>
        <el-divider direction="vertical"></el-divider>
        <span :class="'div-level '+ item.level">{{item.level}}</span>
        <el-divider direction="vertical"></el-divider>
        <span style="white-space: pre;">{{item.message}}</span>
      </div>
      <br />
    </div>
    <el-dialog
      :visible.sync="dialogVisible"
      width="80%"
      :show-close="false"
      :destroy-on-close="true"
    >
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
      title: "",
      message: "",
      from: 0,
      size: 20
    };
  },
  methods: {
    async search(index) {
       
    }
  },
  async mounted() {}
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


