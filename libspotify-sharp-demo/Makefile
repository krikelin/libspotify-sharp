
EXTRA_DIST =  Makefile

# Warning: This is an automatically generated file, do not edit!

srcdir=.
top_srcdir=..

include $(top_srcdir)/config.make

ifeq ($(CONFIG),DEBUG)
ASSEMBLY_COMPILER_COMMAND = gmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:4 -optimize- -debug "-define:DEBUG"
ASSEMBLY = bin/Debug/demo.exe
ASSEMBLY_MDB = $(ASSEMBLY).mdb
COMPILE_TARGET = exe
PROJECT_REFERENCES =  \
	../libspotify-sharp/bin/Debug/libspotify-sharp.dll
BUILD_DIR = bin/Debug

BASS_DLL_SOURCE=bass.dll
LIBSPOTIFY_SHARP_DLL_SOURCE=../libspotify-sharp/bin/Debug/libspotify-sharp.dll
BASS_NET_DLL_SOURCE=Bass.Net.dll
LIBSPOTIFY_DLL_SOURCE=libspotify.dll
DEMO_EXE_MDB_SOURCE=bin/Debug/demo.exe.mdb
DEMO_EXE_MDB=$(BUILD_DIR)/demo.exe.mdb
LIBSPOTIFY_SHARP_DLL_MDB_SOURCE=../libspotify-sharp/bin/Debug/libspotify-sharp.dll.mdb
LIBSPOTIFY_SHARP_DLL_MDB=$(BUILD_DIR)/libspotify-sharp.dll.mdb

endif

ifeq ($(CONFIG),RELEASE)
ASSEMBLY_COMPILER_COMMAND = gmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:4 -optimize-
ASSEMBLY = bin/Release/libspotify-sharp-demo.exe
ASSEMBLY_MDB = 
COMPILE_TARGET = exe
PROJECT_REFERENCES =  \
	../libspotify-sharp/bin/Release/libspotify-sharp.dll
BUILD_DIR = bin/Release

BASS_DLL_SOURCE=bass.dll
LIBSPOTIFY_SHARP_DLL_SOURCE=../libspotify-sharp/bin/Release/libspotify-sharp.dll
BASS_NET_DLL_SOURCE=Bass.Net.dll
LIBSPOTIFY_DLL_SOURCE=libspotify.dll
DEMO_EXE_MDB=
LIBSPOTIFY_SHARP_DLL_MDB=

endif

AL=al2
SATELLITE_ASSEMBLY_NAME=$(notdir $(basename $(ASSEMBLY))).resources.dll

PROGRAMFILES = \
	$(BASS_DLL) \
	$(LIBSPOTIFY_SHARP_DLL) \
	$(BASS_NET_DLL) \
	$(LIBSPOTIFY_DLL) \
	$(DEMO_EXE_MDB) \
	$(LIBSPOTIFY_SHARP_DLL_MDB)  


RESGEN=resgen2

BASS_DLL = $(BUILD_DIR)/bass.dll
LIBSPOTIFY_SHARP_DLL = $(BUILD_DIR)/libspotify-sharp.dll
BASS_NET_DLL = $(BUILD_DIR)/Bass.Net.dll
LIBSPOTIFY_DLL = $(BUILD_DIR)/libspotify.dll

FILES = \
	BASSPlayer.cs \
	Player.cs \
	Main.cs \
	AssemblyInfo.cs \
	AlsaPlayer.cs \
	AudioData.cs 

DATA_FILES = 

RESOURCES = 

EXTRAS = 

REFERENCES =  \
	System \
	System.Drawing

DLL_REFERENCES =  \
	Bass.Net.dll

CLEANFILES = $(PROGRAMFILES) 

#Targets
all: $(ASSEMBLY) $(PROGRAMFILES)  $(top_srcdir)/config.make

include $(top_srcdir)/Makefile.include
#include $(srcdir)/custom-hooks.make



$(eval $(call emit-deploy-target,BASS_DLL))
$(eval $(call emit-deploy-target,LIBSPOTIFY_SHARP_DLL))
$(eval $(call emit-deploy-target,BASS_NET_DLL))
$(eval $(call emit-deploy-target,LIBSPOTIFY_DLL))
$(eval $(call emit-deploy-target,LIBSPOTIFY_SHARP_DLL_MDB))


$(eval $(call emit_resgen_targets))
$(build_xamlg_list): %.xaml.g.cs: %.xaml
	xamlg '$<'


$(ASSEMBLY) $(ASSEMBLY_MDB): $(build_sources) $(build_resources) $(build_datafiles) $(DLL_REFERENCES) $(PROJECT_REFERENCES) $(build_xamlg_list) $(build_satellite_assembly_list)
	make pre-all-local-hook prefix=$(prefix)
	mkdir -p $(shell dirname $(ASSEMBLY))
	make $(CONFIG)_BeforeBuild
	$(ASSEMBLY_COMPILER_COMMAND) $(ASSEMBLY_COMPILER_FLAGS) -out:$(ASSEMBLY) -target:$(COMPILE_TARGET) $(build_sources_embed) $(build_resources_embed) $(build_references_ref)
	make $(CONFIG)_AfterBuild
	make post-all-local-hook prefix=$(prefix)

install-local: $(ASSEMBLY) $(ASSEMBLY_MDB)
	make pre-install-local-hook prefix=$(prefix)
	make install-satellite-assemblies prefix=$(prefix)
	mkdir -p '$(DESTDIR)$(libdir)/$(PACKAGE)'
	$(call cp,$(ASSEMBLY),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call cp,$(ASSEMBLY_MDB),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call cp,$(BASS_DLL),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call cp,$(LIBSPOTIFY_SHARP_DLL),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call cp,$(BASS_NET_DLL),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call cp,$(LIBSPOTIFY_DLL),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call cp,$(DEMO_EXE_MDB),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call cp,$(LIBSPOTIFY_SHARP_DLL_MDB),$(DESTDIR)$(libdir)/$(PACKAGE))
	make post-install-local-hook prefix=$(prefix)

uninstall-local: $(ASSEMBLY) $(ASSEMBLY_MDB)
	make pre-uninstall-local-hook prefix=$(prefix)
	make uninstall-satellite-assemblies prefix=$(prefix)
	$(call rm,$(ASSEMBLY),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call rm,$(ASSEMBLY_MDB),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call rm,$(BASS_DLL),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call rm,$(LIBSPOTIFY_SHARP_DLL),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call rm,$(BASS_NET_DLL),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call rm,$(LIBSPOTIFY_DLL),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call rm,$(DEMO_EXE_MDB),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call rm,$(LIBSPOTIFY_SHARP_DLL_MDB),$(DESTDIR)$(libdir)/$(PACKAGE))
	make post-uninstall-local-hook prefix=$(prefix)

install: install-local
uninstall: uninstall-local
clean: clean-local

include $(top_srcdir)/rules.make
