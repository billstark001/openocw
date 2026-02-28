<template>
  <PageFrame>
    <div class="search-page">
      <div class="search-header">
        <h2>{{ t('search.title') }}</h2>
        <p class="search-subtitle">
          {{ t('search.query') }}: <strong>{{ queryText }}</strong>
          <span v-if="totalCount !== null"> ({{ totalCount }})</span>
        </p>
      </div>
      <CourseList :query="searchResult" />
      <div class="pagination" v-if="totalPages > 1">
        <button class="round-button s-hollow h" @click="changePage(page - 1)" :disabled="page <= 1">
          {{ t('search.hint.prev') }}
        </button>
        <span class="page-info">{{ page }} / {{ totalPages }}</span>
        <button class="round-button s-hollow h" @click="changePage(page + 1)" :disabled="page >= totalPages">
          {{ t('search.hint.next') }}
        </button>
      </div>
    </div>
  </PageFrame>
  <PageFooter />
</template>

<script setup lang="ts">
import { ref, watch, onMounted, computed } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { useI18n } from 'vue-i18n';
import PageFrame from '@/components/PageFrame.vue';
import PageFooter from '@/components/lesser/PageFooter.vue';
import CourseList from '@/components/courses/CourseList.vue';
import { searchCourses, CourseBrief, SearchParams } from '@/api/query';
import { QueryResult } from '@/utils/query';

const { t, locale } = useI18n();
const route = useRoute();
const router = useRouter();

const PAGE_SIZE = 20;

const searchResult = ref<QueryResult<CourseBrief[]> | undefined>(undefined);
const page = ref(1);
const totalPages = ref(1);
const totalCount = ref<number | null>(null);

const queryText = computed(() => route.query.q as string || '');
const mode = computed(() => route.query.mode as string || 'kw');

function buildParams(): SearchParams {
  const q = queryText.value;
  const m = mode.value;
  if (m === 'code') return { codeVague: q, page: page.value, pageSize: PAGE_SIZE };
  if (m === 'name') return { infoVague: q, page: page.value, pageSize: PAGE_SIZE };
  return { contentVague: q, page: page.value, pageSize: PAGE_SIZE };
}

async function doSearch() {
  if (!queryText.value) return;
  const result = await searchCourses(buildParams(), locale.value);
  searchResult.value = result;
  if (result.totalPage) totalPages.value = result.totalPage;
  totalCount.value = result.totalCount ?? result.result?.length ?? null;
}

function changePage(p: number) {
  if (p < 1 || p > totalPages.value) return;
  page.value = p;
  router.replace({ query: { ...route.query, page: String(p) } });
}

watch([queryText, mode], () => {
  page.value = 1;
  doSearch();
});

onMounted(() => {
  const p = parseInt(route.query.page as string);
  if (!isNaN(p)) page.value = p;
  doSearch();
});
</script>

<style scoped>
.search-page {
  padding: 24px;
  min-height: 400px;
}
.search-header {
  margin-bottom: 16px;
}
.search-subtitle {
  color: var(--color-txt3);
  margin-top: 4px;
}
.pagination {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 12px;
  padding: 24px 0;
}
.page-info {
  color: var(--color-txt1);
}
</style>
