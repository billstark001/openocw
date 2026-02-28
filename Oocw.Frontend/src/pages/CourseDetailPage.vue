<template>
  <PageBanner />
  <PageFrame>
    <div class="course-detail" v-if="course">
      <div class="course-heading">
        <h1>{{ course.name }}</h1>
        <p class="course-meta">
          <span>{{ t('course.code') }}: {{ course.courseCode }}</span>
          <span>{{ t('course.credit') }}: {{ course.credit }}</span>
        </p>
      </div>

      <div class="chip-row" v-if="course.tags.length">
        <span class="round-button s-sub" v-for="tag in course.tags" :key="tag">{{ tag }}</span>
      </div>

      <div class="chip-row" v-if="course.lecturers.length">
        <strong>{{ t('course.lecturers') }}:</strong>
        <span class="round-button s-hollow" v-for="l in course.lecturers" :key="l">{{ l }}</span>
      </div>

      <div class="chip-row" v-if="course.departments.length">
        <strong>{{ t('course.departments') }}:</strong>
        <span class="round-button s-hollow" v-for="d in course.departments" :key="d">{{ d }}</span>
      </div>

      <div class="section" v-if="course.content">
        <h3>{{ t('course.content') }}</h3>
        <pre class="syllabus">{{ course.content }}</pre>
      </div>

      <div class="section">
        <h3>{{ t('course.classes') }}</h3>
        <div v-if="classes.length === 0" class="empty-msg">{{ t('course.noClasses') }}</div>
        <div v-for="cls in classes" :key="cls.id" class="class-row">
          <router-link :to="`/class-instance/${cls.id}`" class="round-button s-hollow h">
            {{ cls.name || cls.id }}
          </router-link>
        </div>
      </div>
    </div>
    <div v-else class="loading-area">
      <p>{{ loadingMsg }}</p>
    </div>
  </PageFrame>
  <PageFooter />
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { useRoute } from 'vue-router';
import { useI18n } from 'vue-i18n';
import PageFrame from '@/components/PageFrame.vue';
import PageBanner from '@/components/lesser/PageBanner.vue';
import PageFooter from '@/components/lesser/PageFooter.vue';
import { getCourseInfo, CourseDetail } from '@/api/query';
import { getInfo } from '@/utils/query';

const { t, locale } = useI18n();
const route = useRoute();

const course = ref<CourseDetail | null>(null);
const classes = ref<{ id: string; name: string }[]>([]);
const loadingMsg = ref('Loading...');

onMounted(async () => {
  const code = route.params.code as string;
  const res = await getCourseInfo(code, undefined, locale.value);
  if (res.status === 200 && res.result) {
    course.value = res.result;
    // fetch class list
    const clsRes = await getInfo<{ list: { id: string; name: string }[] }>(
      `/api/class/list?courseId=${encodeURIComponent(res.result.id)}`
    );
    if (clsRes.status === 200 && clsRes.result) {
      classes.value = clsRes.result.list;
    }
  } else {
    loadingMsg.value = `Error ${res.status}: ${res.info}`;
  }
});
</script>

<style scoped>
.course-detail {
  padding: 24px;
}
.course-heading {
  margin-bottom: 16px;
}
.course-meta span {
  margin-right: 16px;
  color: var(--color-txt3);
}
.chip-row {
  display: flex;
  flex-wrap: wrap;
  gap: 6px;
  align-items: center;
  margin: 8px 0;
}
.section {
  margin-top: 24px;
}
.section h3 {
  margin-bottom: 8px;
}
.syllabus {
  white-space: pre-wrap;
  font-family: inherit;
  background-color: var(--color-bg-trs);
  padding: 12px;
  border-radius: 6px;
}
.class-row {
  margin: 6px 0;
}
.empty-msg {
  color: var(--color-txt3);
}
.loading-area {
  padding: 40px;
  text-align: center;
}
</style>
