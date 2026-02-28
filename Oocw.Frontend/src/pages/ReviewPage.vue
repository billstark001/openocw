<template>
  <PageFrame>
    <div class="review-page">
      <h2>{{ t('review.title') }}</h2>

      <div v-if="items.length === 0" class="empty-msg">{{ t('review.noItems') }}</div>

      <div v-for="item in items" :key="item.systemId" class="review-item">
        <div class="review-meta">
          <span><strong>{{ t('review.sender') }}:</strong> {{ item.senderId }}</span>
          <span><strong>{{ t('review.target') }}:</strong> {{ item.targetCollection }} / {{ item.targetObjectId }}</span>
          <span class="time">{{ item.createTime }}</span>
        </div>
        <div class="review-actions">
          <input type="text" v-model="comments[item.systemId]" class="text-input"
            :placeholder="t('review.comment')" />
          <button class="round-button s-main h" @click="approve(item.systemId)">{{ t('btn.approve') }}</button>
          <button class="round-button s-hollow h" @click="reject(item.systemId)">{{ t('btn.reject') }}</button>
        </div>
      </div>

      <div class="pagination" v-if="totalPages > 1">
        <button class="round-button s-hollow h" @click="changePage(page - 1)" :disabled="page <= 1">
          {{ t('search.hint.prev') }}
        </button>
        <span>{{ page }} / {{ totalPages }}</span>
        <button class="round-button s-hollow h" @click="changePage(page + 1)" :disabled="page >= totalPages">
          {{ t('search.hint.next') }}
        </button>
      </div>
    </div>
  </PageFrame>
  <PageFooter />
</template>

<script setup lang="ts">
import { ref, reactive, onMounted } from 'vue';
import { useI18n } from 'vue-i18n';
import PageFrame from '@/components/PageFrame.vue';
import PageFooter from '@/components/lesser/PageFooter.vue';
import { getReviewQueue, approveRequest, rejectRequest, ReviewRequest } from '@/api/admin';

const { t } = useI18n();

const items = ref<ReviewRequest[]>([]);
const comments = reactive<Record<string, string>>({});
const page = ref(1);
const totalPages = ref(1);

async function load() {
  const res = await getReviewQueue(page.value, 20);
  if (res.status === 200 && res.result) {
    items.value = res.result.list;
    totalPages.value = res.result.totalPage || 1;
    res.result.list.forEach(i => { comments[i.systemId] = ''; });
  }
}

async function approve(id: string) {
  await approveRequest(id, comments[id]);
  await load();
}

async function reject(id: string) {
  await rejectRequest(id, comments[id]);
  await load();
}

function changePage(p: number) {
  if (p < 1 || p > totalPages.value) return;
  page.value = p;
  load();
}

onMounted(load);
</script>

<style scoped>
.review-page { padding: 24px; }
.empty-msg { color: var(--color-txt3); padding: 24px 0; }
.review-item { padding: 16px; margin: 12px 0; background: var(--color-bg-trs); border-radius: 6px; }
.review-meta { display: flex; flex-wrap: wrap; gap: 12px; margin-bottom: 10px; color: var(--color-txt1); }
.time { color: var(--color-txt3); font-size: smaller; }
.review-actions { display: flex; align-items: center; gap: 8px; }
.text-input { flex: 1; padding: 6px 10px; border: 1px solid #ccc; border-radius: 4px; background: transparent; color: var(--color-txt1); }
.pagination { display: flex; gap: 12px; justify-content: center; padding: 24px 0; }
</style>
