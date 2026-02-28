<template>
  <PageFrame>
    <div class="notification-page">
      <div class="page-header">
        <h2>{{ t('notification.title') }}</h2>
        <button class="round-button s-main h" @click="readAll">{{ t('notification.markAllRead') }}</button>
      </div>

      <div v-if="items.length === 0" class="empty-msg">{{ t('notification.noItems') }}</div>

      <div v-for="item in items" :key="item.systemId" :class="['notif-item', item.read ? 'read' : 'unread']">
        <div class="notif-body">
          <p class="notif-msg">{{ getMsg(item) }}</p>
          <p class="notif-time">{{ item.createTime }}</p>
        </div>
        <div class="notif-actions">
          <span class="round-button s-sub">{{ item.read ? t('notification.read') : t('notification.unread') }}</span>
          <button v-if="!item.read" class="round-button s-hollow h" @click="markOne(item.systemId)">
            {{ t('notification.markRead') }}
          </button>
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
import { ref, onMounted } from 'vue';
import { useI18n } from 'vue-i18n';
import PageFrame from '@/components/PageFrame.vue';
import PageFooter from '@/components/lesser/PageFooter.vue';
import { listNotifications, markRead, markAllRead, NotificationItem } from '@/api/notification';

const { t, locale } = useI18n();

const items = ref<NotificationItem[]>([]);
const page = ref(1);
const totalPages = ref(1);

function getMsg(item: NotificationItem): string {
  return item.message[locale.value] || item.message['en'] || Object.values(item.message)[0] || '';
}

async function load() {
  const res = await listNotifications(false, page.value, 20);
  if (res.status === 200 && res.result) {
    items.value = res.result.list;
    totalPages.value = res.result.totalPage || 1;
  }
}

async function markOne(id: string) {
  await markRead(id);
  await load();
}

async function readAll() {
  await markAllRead();
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
.notification-page { padding: 24px; }
.page-header { display: flex; align-items: center; justify-content: space-between; margin-bottom: 16px; }
.notif-item { display: flex; justify-content: space-between; align-items: center; padding: 12px; margin: 8px 0; border-radius: 6px; background: var(--color-bg-trs); }
.notif-item.unread { border-left: 4px solid var(--color-s-acc-main); }
.notif-msg { margin-bottom: 4px; }
.notif-time { font-size: smaller; color: var(--color-txt3); }
.notif-actions { display: flex; gap: 8px; align-items: center; }
.empty-msg { color: var(--color-txt3); padding: 24px 0; }
.pagination { display: flex; gap: 12px; justify-content: center; padding: 24px 0; }
</style>
