
EXTRA_DIST =  Makefile

# Warning: This is an automatically generated file, do not edit!

srcdir=.
top_srcdir=..

include $(top_srcdir)/config.make

ifeq ($(CONFIG),DEBUG)
ASSEMBLY_COMPILER_COMMAND = gmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:3 -optimize- -debug "-define:DEBUG"
ASSEMBLY = bin/Debug/libspotify-sharp.dll
ASSEMBLY_MDB = $(ASSEMBLY).mdb
COMPILE_TARGET = library
PROJECT_REFERENCES = 
BUILD_DIR = bin/Debug

LIBSPOTIFY_SHARP_DLL_MDB_SOURCE=bin/Debug/libspotify-sharp.dll.mdb
LIBSPOTIFY_SHARP_DLL_MDB=$(BUILD_DIR)/libspotify-sharp.dll.mdb

endif

ifeq ($(CONFIG),RELEASE)
ASSEMBLY_COMPILER_COMMAND = gmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:4 -optimize-
ASSEMBLY = bin/Release/libspotify-sharp.dll
ASSEMBLY_MDB = 
COMPILE_TARGET = library
PROJECT_REFERENCES = 
BUILD_DIR = bin/Release

LIBSPOTIFY_SHARP_DLL_MDB=

endif

AL=al2
SATELLITE_ASSEMBLY_NAME=$(notdir $(basename $(ASSEMBLY))).resources.dll

PROGRAMFILES = \
	$(LIBSPOTIFY_SHARP_DLL_MDB)  

LINUX_PKGCONFIG = \
	$(LIBSPOTIFY_SHARP_PC)  


RESGEN=resgen2

LIBSPOTIFY_SHARP_PC = $(BUILD_DIR)/libspotify-sharp.pc

FILES = \
	Properties/AssemblyInfo.cs \
	src/Events/AlbumBrowseEventArgs.cs \
	src/Events/ArtistBrowseEventArgs.cs \
	src/Events/EventWorkItem.cs \
	src/Events/MusicDeliveryEventArgs.cs \
	src/Events/PlaylistContainerEventArgs.cs \
	src/Events/PlaylistEventArgs.cs \
	src/Events/SearchEventArgs.cs \
	src/Events/SessionEventArgs.cs \
	src/Events/TracksEventArgs.cs \
	src/Album.cs \
	src/AlbumBrowse.cs \
	src/Artist.cs \
	src/ArtistBrowse.cs \
	src/libspotify.cs \
	src/Link.cs \
	src/PlayList.cs \
	src/PlaylistContainer.cs \
	src/Search.cs \
	src/Session.cs \
	src/SpotifyException.cs \
	src/Track.cs \
	src/User.cs \
	src/Events/ImageEventArgs.cs 

DATA_FILES = 

RESOURCES = 

EXTRAS = \
	src \
	libspotify-sharp.pc.in 

REFERENCES =  \
	System \
	System.Drawing

DLL_REFERENCES = 

CLEANFILES = $(PROGRAMFILES) $(LINUX_PKGCONFIG) 

#Targets
all: $(ASSEMBLY) $(PROGRAMFILES) $(LINUX_PKGCONFIG)  $(top_srcdir)/config.make

include $(top_srcdir)/Makefile.include
#include $(srcdir)/custom-hooks.make



$(eval $(call emit-deploy-wrapper,LIBSPOTIFY_SHARP_PC,libspotify-sharp.pc))


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
	$(call cp,$(LIBSPOTIFY_SHARP_DLL_MDB),$(DESTDIR)$(libdir)/$(PACKAGE))
	mkdir -p '$(DESTDIR)$(libdir)/pkgconfig'
	$(call cp,$(LIBSPOTIFY_SHARP_PC),$(DESTDIR)$(libdir)/pkgconfig)
	make post-install-local-hook prefix=$(prefix)

uninstall-local: $(ASSEMBLY) $(ASSEMBLY_MDB)
	make pre-uninstall-local-hook prefix=$(prefix)
	make uninstall-satellite-assemblies prefix=$(prefix)
	$(call rm,$(ASSEMBLY),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call rm,$(ASSEMBLY_MDB),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call rm,$(LIBSPOTIFY_SHARP_DLL_MDB),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call rm,$(LIBSPOTIFY_SHARP_PC),$(DESTDIR)$(libdir)/pkgconfig)
	make post-uninstall-local-hook prefix=$(prefix)

install: install-local
uninstall: uninstall-local
clean: clean-local

include $(top_srcdir)/rules.make
